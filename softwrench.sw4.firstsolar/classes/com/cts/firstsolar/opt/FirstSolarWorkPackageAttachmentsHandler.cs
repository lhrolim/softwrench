using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.Util;
using log4net;
using Newtonsoft.Json.Linq;
using NHibernate.Util;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
using softWrench.sW4.Metadata;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {
    public class FirstSolarWorkPackageAttachmentsHandler : ISingletonComponent {

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public AttachmentHandler AttachmentHandler { get; set; }

        [Import]
        public IEmailService EmailService { get; set; }

        protected static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarWorkPackageAttachmentsHandler));

        public void HandleAttachments(CrudOperationData crudoperationData, long id, string relationship, string filterPrefix, CrudOperationData woData) {
            if (crudoperationData.UnmappedAttributes == null || !crudoperationData.UnmappedAttributes.ContainsKey(relationship)) {
                return;
            }

            var attachsStr = crudoperationData.UnmappedAttributes[relationship];
            if (string.IsNullOrEmpty(attachsStr)) {
                return;
            }

            var attachs = JArray.Parse("[" + attachsStr + "]");
            attachs.ForEach(attach => {
                var attachObj = attach as JObject;
                if (attachObj != null) {
                    HandleAttachment(attachObj, id, filterPrefix, woData);
                }
            });
        }

        public void HandleAttachmentsOnCompositionLoad(CompositionFetchResult woResult, CompositionFetchResult packageResult, string localRelationship, string woRelationship) {
            var attachs = woResult.ResultObject.FirstOrDefault(pair => woRelationship.Equals(pair.Key)).Value;
            if (attachs == null) {
                //might be null due to security policies
                return;
            }


            var attachsMap = new Dictionary<string, IList<Dictionary<string, object>>>();
            attachs.ResultList.ForEach(attach => {
                if (!attachsMap.ContainsKey(localRelationship)) {
                    attachsMap.Add(localRelationship, new List<Dictionary<string, object>>());
                }
                attachsMap[localRelationship].Add(attach);
            });

            attachsMap.ForEach(pair => {
                var searchResult = new EntityRepository.SearchEntityResult {
                    ResultList = pair.Value,
                    IdFieldName = attachs.IdFieldName,
                    PaginationData = attachs.PaginationData
                };
                packageResult.ResultObject.Add(pair.Key, searchResult);
            });
        }

        public async Task AddEmailAttachment(IDictionary<string, object> attach, List<EmailAttachment> emailAttachs) {
            var tuple = await AttachmentHandler.DownloadViaHttpById(attach["docinfoid"].ToString());
            var emailAttach = EmailService.CreateAttachment(tuple.Item1, attach["docinfo_.description"].ToString());
            emailAttachs.Add(emailAttach);
        }

        private static void HandleAttachment(JObject attachment, long id, string filterPrefix, CrudOperationData woData) {
            var isNew = TryGetValue(attachment, "#newFile");
            if (!"true".EqualsIc(isNew)) {
                return;
            }

            var value = attachment.GetValue("value").Value<string>();
            var label = attachment.GetValue("label").Value<string>();

            // create the attachment to save
            var toSaveAttachment = new JObject {
                new JProperty("#isDirty", true),
                new JProperty("createdate", null),
                new JProperty("docinfo_.description", label),
                new JProperty("document", Guid.NewGuid().ToString().Substring(0,20)),
                new JProperty("#filter", filterPrefix + id),
                new JProperty("newattachment", value),
                new JProperty("newattachment_path", label),
                new JProperty("_iscreation", true)
            };

            var entity = MetadataProvider.Entity("DOCLINKS");
            var app = MetadataProvider.Application("attachment");
            var appMetadata = app.StaticFromSchema("list");
            var attachCrudOperationData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), entity, appMetadata, toSaveAttachment);

            if (!woData.AssociationAttributes.ContainsKey("attachment_")) {
                woData.AssociationAttributes.Add("attachment_", new List<CrudOperationData>());
            }
            var attachList = woData.AssociationAttributes["attachment_"] as List<CrudOperationData>;
            if (attachList == null) {
                throw new Exception("Failed to build attachment list.");
            }
            attachList.Add(attachCrudOperationData);
        }

        private static string TryGetValue(JObject obj, string key) {
            JToken token;
            obj.TryGetValue(key, out token);
            return token?.Value<string>();
        }

        public async Task HandleEmails<T>(WorkPackage package, string siteId, string attachsRelationship, string filterPrefix, IEnumerable<T> requests, FirstSolarBaseEmailService<T> emailService) where T : IFsEmailRequest {
            var requestsList = requests as IList<T> ?? requests.ToList();

            if (!requestsList.Any()) {
                return;
            }

            // to avoid cicle
            var dataset = SimpleInjectorGenericFactory.Instance.GetObject<FirstSolarWorkPackageDataSet>();

            var relList = new List<string> { attachsRelationship };
            var wonum = package.Wonum;
            var woCompositions = await dataset.GetWoCompositions(package.WorkorderId.ToString(), wonum, siteId, relList);
            var attachs = woCompositions.ResultObject.First(pair => attachsRelationship.Equals(pair.Key)).Value;
            requestsList.ForEach(request => {
                AsyncHelper.RunSync(() => InnerHandleEmail(request, package, siteId, filterPrefix, attachs, emailService));
            });
        }

        public void HandleEmail<T>(T request, WorkPackage package, string siteId, string filterPrefix, string relationship, FirstSolarBaseEmailService<T> emailService) where T : IFsEmailRequest {
            // to avoid cicle
            var dataset = SimpleInjectorGenericFactory.Instance.GetObject<FirstSolarWorkPackageDataSet>();

            var relList = new List<string> { relationship };
            var woCompositions = AsyncHelper.RunSync(() => dataset.GetWoCompositions(package.WorkorderId + "", package.Wonum, siteId, relList));
            var attachs = woCompositions.ResultObject.First(pair => relationship.Equals(pair.Key)).Value;
            AsyncHelper.RunSync(() => InnerHandleEmail(request, package, siteId, filterPrefix, attachs, emailService));
        }

        private async Task InnerHandleEmail<T>(T request, WorkPackage package, string siteId, string filterPrefix, EntityRepository.SearchEntityResult attachs, FirstSolarBaseEmailService<T> emailService) where T : IFsEmailRequest {
            var emailAttachs = new List<EmailAttachment>();
            var attachTasks = new List<Task>();

            try {
                attachs.ResultList.ForEach(attach => {
                    var filter = filterPrefix + request.Id;
                    if (filter.Equals(attach["docinfo_.urlparam1"])) {
                        attachTasks.Add(AddEmailAttachment(attach, emailAttachs));
                    }
                });

                await Task.WhenAll(attachTasks);

                await emailService.SendEmail(request, package, siteId, emailAttachs);
            } catch (Exception ex) {
                request.Status = RequestStatus.Error;
                Log.ErrorFormat("Failed to send email for {0} {1} from workorder with wonum {2} from site {3}: {4}", emailService.RequestI18N(), request.Id, package.Wonum, siteId, ex.Message);
                await Dao.SaveAsync(request as IFsEmailRequest);
            }
        }
    }
}
