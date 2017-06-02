using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.Util;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Data.Persistence.Operation;
using NHibernate.Util;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {
    public class FirstSolarCallOutHandler : ISingletonComponent {

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public IMaximoHibernateDAO MaxDao { get; set; }

        [Import]
        public FirstSolarCallOutEmailService CallOutEmailService { get; set; }

        [Import]
        public IEmailService EmailService { get; set; }

        [Import]
        public AttachmentHandler AttachmentHandler { get; set; }

        private const string FilterPrefix = "swwpkgco:";
        protected static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarCallOutHandler));

        public void HandleCalloutAttachments(CompositionFetchResult woResult, CompositionFetchResult packageResult) {
            var callOutAttachs = woResult.ResultObject.First(pair => FSWPackageConstants.CallOutAttachsRelationship.Equals(pair.Key)).Value;
            const string relationship = "#calloutfileexplorer_";

            var attachsMap = new Dictionary<string, IList<Dictionary<string, object>>>();
            callOutAttachs.ResultList.ForEach(attach => {
                if (!attachsMap.ContainsKey(relationship)) {
                    attachsMap.Add(relationship, new List<Dictionary<string, object>>());
                }
                attachsMap[relationship].Add(attach);
            });

            attachsMap.ForEach(pair => {
                var searchResult = new EntityRepository.SearchEntityResult {
                    ResultList = pair.Value,
                    IdFieldName = callOutAttachs.IdFieldName,
                    PaginationData = callOutAttachs.PaginationData
                };
                packageResult.ResultObject.Add(pair.Key, searchResult);
            });
        }

        public void HandleCallOuts(CrudOperationData crudoperationData, WorkPackage package, CrudOperationData woData) {
            var existingCallOuts = package.CallOuts;
            package.CallOuts = new List<CallOut>();
            if (crudoperationData.AssociationAttributes != null && crudoperationData.AssociationAttributes.ContainsKey("callOuts_")) {
                var callOutsData = crudoperationData.AssociationAttributes["callOuts_"] as List<CrudOperationData>;
                if (callOutsData == null) {
                    throw new Exception("Incorrect format of subcontractors call out list.");
                }
                callOutsData.ForEach((data) => {
                    package.CallOuts.Add(HandleCallout(data, GetOurCreateCallOut(data, existingCallOuts), package, woData));
                });
            }
            existingCallOuts?.ForEach(callout => {
                if (RequestStatus.Sent.Equals(callout.Status)) {
                    throw new Exception("Is not possible delete a sent subcontractor callout. Reload the page to get the updated version of this work package.");
                }
                Dao.Delete(callout);
            });
        }

        public async Task<bool> HandleEmails(WorkPackage package, string siteId, IEnumerable<CallOut> calloutsToSend){
            var callOuts = calloutsToSend as IList<CallOut> ?? calloutsToSend.ToList();

            if (!callOuts.Any()) {
                return false;
            }

            // to avoid cicle
            var dataset = SimpleInjectorGenericFactory.Instance.GetObject<FirstSolarWorkPackageDataSet>();

            var relList = new List<string> { FSWPackageConstants.CallOutAttachsRelationship };
            var wonum = package.Wonum;
            var woCompositions = await dataset.GetWoCompositions(package.WorkorderId.ToString(), wonum, siteId, relList);
            var callOutAttachs = woCompositions.ResultObject.First(pair => FSWPackageConstants.CallOutAttachsRelationship.Equals(pair.Key)).Value;
            callOuts.ForEach(callOut => {
                AsyncHelper.RunSync(() => InnerHandleEmail(callOut, wonum, siteId, callOutAttachs));
            });

            return true;
        }

        public void HandleEmail(CallOut callOut, string woId, string woNum, string siteId) {
            // to avoid cicle
            var dataset = SimpleInjectorGenericFactory.Instance.GetObject<FirstSolarWorkPackageDataSet>();

            var relList = new List<string> { FSWPackageConstants.CallOutAttachsRelationship };
            var woCompositions = AsyncHelper.RunSync(() => dataset.GetWoCompositions(woId, woNum, siteId, relList));
            var callOutAttachs = woCompositions.ResultObject.First(pair => FSWPackageConstants.CallOutAttachsRelationship.Equals(pair.Key)).Value;
            AsyncHelper.RunSync(() => InnerHandleEmail(callOut, woNum, siteId, callOutAttachs));
        }

        private async Task InnerHandleEmail(CallOut callOut, string wonum, string siteId, EntityRepository.SearchEntityResult callOutAttachs) {
            var emailAttachs = new List<EmailAttachment>();
            var attachTasks = new List<Task>();

            try {
                callOutAttachs.ResultList.ForEach(attach => {
                    var filter = FilterPrefix + callOut.Id;
                    if (filter.Equals(attach["docinfo_.urlparam1"])) {
                        attachTasks.Add(AddEmailAttachment(attach, emailAttachs));
                    }
                });

                await Task.WhenAll(attachTasks);

                await CallOutEmailService.SendCallout(callOut, callOut.Email, emailAttachs);
            } catch (Exception ex) {
                callOut.Status = RequestStatus.Error;
                Log.ErrorFormat("Failed to send email for callout {0} from workorder with wonum {1} from site {2}: {3}", callOut.Id, wonum, siteId, ex.Message);
                await Dao.SaveAsync(callOut);
            }
        }

        private async Task AddEmailAttachment(IDictionary<string, object> attach, List<EmailAttachment> emailAttachs) {
            var tuple = await AttachmentHandler.DownloadViaHttpById(attach["docinfoid"].ToString());
            var emailAttach = EmailService.CreateAttachment(tuple.Item1, attach["docinfo_.description"].ToString());
            emailAttachs.Add(emailAttach);
        }

        private CallOut HandleCallout(CrudOperationData crudoperationData, CallOut callOut, WorkPackage workpackage, CrudOperationData woData) {
            var newStatusSt = crudoperationData.GetStringAttribute("status");
            var sendNow = crudoperationData.GetBooleanAttribute("sendnow");

            RequestStatus newStatus;
            Enum.TryParse(newStatusSt, true, out newStatus);

            if (RequestStatus.Sent.Equals(callOut.Status)) {
                if (!RequestStatus.Sent.Equals(newStatus)) {
                    throw new Exception("Is not possible edit a sent subcontractor callout. Reload the page to get the updated version of this work package.");
                }
                // submited callouts are not editable so just return the existing one
                return callOut;
            }

            var subcontractor = crudoperationData.AssociationAttributes["subcontractor_"] as CrudOperationData;
            if (subcontractor == null) {
                throw new Exception("Missing subcontractor.");
            }
            var nullableSubcontractorId = subcontractor.GetIntAttribute("id");
            if (nullableSubcontractorId == null) {
                throw new Exception("Missing subcontractor id.");
            }
            callOut.SubContractor = Dao.FindByPK<SubContractor>(typeof(SubContractor), nullableSubcontractorId.Value);

            callOut.Status = newStatus;

            if (sendNow.HasValue && sendNow.Value) {
                callOut.SendTime = DateTime.Now.FromServerToMaximo();
                callOut.SendNow = true;
            } else {
                callOut.Status = RequestStatus.Scheduled;
                var dateFromJson = Convert.ToDateTime(crudoperationData.GetStringAttribute("sendTime"), new CultureInfo("en-US"));
                callOut.SendTime = dateFromJson.FromUserToMaximo(SecurityFacade.CurrentUser());
            }

            callOut.ExpirationDate = ConversionUtil.HandleDateConversion(crudoperationData.GetStringAttribute("expirationdate"));
            callOut.PoNumber = crudoperationData.GetStringAttribute("ponumber");
            callOut.ToNumber = crudoperationData.GetStringAttribute("tonumber");
            callOut.SiteName = crudoperationData.GetStringAttribute("sitename");
            callOut.Email = crudoperationData.GetStringAttribute("email");
            callOut.BillingEntity = crudoperationData.GetStringAttribute("billingentity");
            callOut.NotToExceedAmount = crudoperationData.GetStringAttribute("nottoexceedamount");
            callOut.RemainingFunds = crudoperationData.GetStringAttribute("remainingfunds");
            callOut.ScopeOfWork = crudoperationData.GetStringAttribute("scopeofwork");
            callOut.PlantContacts = crudoperationData.GetStringAttribute("plantcontacts");
            callOut.OtherInfo = crudoperationData.GetStringAttribute("otherinfo");
            callOut.WorkPackageId = workpackage.Id ?? 0;
            callOut.GenerateToken();

            callOut = Dao.Save(callOut);

            HandleAttachments(crudoperationData, callOut.Id ?? 0, woData);

            return callOut;
        }

        private static CallOut GetOurCreateCallOut(CrudOperationData crudoperationData, IList<CallOut> existingCallOuts) {
            var id = crudoperationData.GetIntAttribute("id");
            if (id == null || existingCallOuts == null) {
                return new CallOut();
            }
            var found = existingCallOuts.FirstOrDefault(callOut => callOut.Id == id);
            if (found == null) {
                return new CallOut() { Id = id };
            }
            existingCallOuts.Remove(found);
            return found;
        }

        private static void HandleAttachments(CrudOperationData crudoperationData, long callOutId, CrudOperationData woData) {
            if (crudoperationData.UnmappedAttributes == null || !crudoperationData.UnmappedAttributes.ContainsKey("#calloutfileexplorer_")) {
                return;
            }

            var attachsStr = crudoperationData.UnmappedAttributes["#calloutfileexplorer_"];
            if (string.IsNullOrEmpty(attachsStr)) {
                return;
            }

            var attachs = JArray.Parse("[" + attachsStr + "]");
            attachs.ForEach(attach => {
                var attachObj = attach as JObject;
                if (attachObj != null) {
                    HandleAttachment(attachObj, callOutId, woData);
                }
            });
        }

        private static void HandleAttachment(JObject attachment, long callOutId, CrudOperationData woData) {
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
                new JProperty("document", label),
                new JProperty("#filter", FilterPrefix + callOutId),
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
    }
}
