using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using log4net;
using NHibernate.Linq;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Maximo;
using softWrench.sW4.Data.PDF;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Email;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.wsWorkorder;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using softWrench.sW4.Metadata.Applications;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {
    public class CommLogHandler : ISingletonComponent {

        private const string Ticketuid = "ticketuid";
        private const string Commloguid = "commloguid";
        private const string Commlog = "commlog";
        private const string Commlogid = "commlogid";
        private const string Ownerid = "ownerid";
        private const string Ownertable = "ownertable";
        private const string Inbound = "inbound";
        private const string Siteid = "siteid";
        private const string Orgid = "orgid";
        private const string Createby = "createby";
        private const string Createdate = "createdate";
        private const string Modifydate = "modifydate";
        private const string Sendto = "sendto";
        private const string Sendfrom = "sendfrom";
        private const string Subject = "subject";
        private const string Message = "message";
        private const string Cc = "cc";
        private const string Bcc = "bcc";


        private readonly ISWDBHibernateDAO _dao;
        private readonly AttachmentHandler _attachmentHandler;
        private readonly PdfService _pdfService;
        private readonly IMemoryContextLookuper _lookuper;

        private static readonly ILog Log = LogManager.GetLogger(typeof(CommLogHandler));


        public CommLogHandler(ISWDBHibernateDAO dao, AttachmentHandler attachmentHandler, PdfService pdfService, IMemoryContextLookuper lookuper) {
            _dao = dao;
            _attachmentHandler = attachmentHandler;
            _pdfService = pdfService;
            _lookuper = lookuper;
            Log.Debug("init");
        }

        public void HandleCommLogs(MaximoOperationExecutionContext maximoTemplateData, CrudOperationData entity, object rootObject) {
            var user = SecurityFacade.CurrentUser();
            var commlogs = (IEnumerable<CrudOperationData>)entity.GetRelationship(Commlog);
            var crudOperationDatas = commlogs as CrudOperationData[] ?? commlogs.ToArray();
            var newCommLogs = crudOperationDatas.Where(r => r.GetAttribute(Commloguid) == null);
            foreach (var commLog in crudOperationDatas) {
                // Convert sendto array to a comma separated list
                HandleArrayOfOptions(commLog, Sendto);
                HandleArrayOfOptions(commLog, Cc);
                HandleArrayOfOptions(commLog, Bcc);
            }
            var ownerid = entity.Id;

            w.CloneArray(newCommLogs, rootObject, "COMMLOG", delegate (object integrationObject, CrudOperationData crudData) {
                var sendTo = w.GetRealValue(integrationObject, Sendto);
                if (sendTo == null) throw new ArgumentNullException("To:");
                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.Add.ToString());
                var id = MaximoHibernateDAO.GetInstance().FindSingleByNativeQuery<object>("Select MAX(commlog.commlogid) from commlog", null);
                var rnd = new Random();
                var commlogid = Convert.ToInt32(id) + rnd.Next(1, 10);
                w.SetValue(integrationObject, Commlogid, commlogid);
                w.SetValue(integrationObject, Ownerid, ownerid);
                w.SetValueIfNull(integrationObject, Ownertable, entity.TableName);
                w.SetValueIfNull(integrationObject, Inbound, false);
                w.CopyFromRootEntity(rootObject, integrationObject, Siteid, user.SiteId);
                w.CopyFromRootEntity(rootObject, integrationObject, Orgid, user.OrgId);
                w.CopyFromRootEntity(rootObject, integrationObject, Createby, user.Login, "CHANGEBY");
                w.SetValue(integrationObject, Createdate, DateTime.Now.FromServerToRightKind());
                w.CopyFromRootEntity(rootObject, integrationObject, Modifydate, DateTime.Now.FromServerToRightKind());
                w.SetValueIfNull(integrationObject, "logtype", "CLIENTNOTE");
                LongDescriptionHandler.HandleLongDescription(integrationObject, crudData);

                var attachments = GetAttachments(crudData, maximoTemplateData.ApplicationMetadata, entity);

                maximoTemplateData.Properties.Add("mailObject", GenerateEmailObject(integrationObject, attachments));
                HandleAttachments(attachments, integrationObject, maximoTemplateData.ApplicationMetadata);
                var username = user.Login;
                var allAddresses = GetListOfAllAddressesUsed(integrationObject);
                UpdateEmailHistoryAsync(username, allAddresses.ToLower().Split(','));
            });
        }

        private string GetListOfAllAddressesUsed(object integrationObject) {
            var recipientEmail = w.GetRealValue(integrationObject, Sendto).ToString();
            var ccEmail = w.GetRealValue(integrationObject, Cc);
            var bccEmail = w.GetRealValue(integrationObject, Bcc);
            ccEmail = ccEmail != null ? ccEmail.ToString() : "";
            bccEmail = bccEmail != null ? bccEmail.ToString() : "";
            var allAddresses = (string)ccEmail != "" ? recipientEmail + "," + ccEmail : recipientEmail;
            allAddresses = (string)bccEmail != "" ? allAddresses + "," + bccEmail : allAddresses;
            return allAddresses;
        }

        private void HandleArrayOfOptions(AttributeHolder commLog, string propertyName) {
            var stringOrArray = commLog.GetAttribute(propertyName);
            if (stringOrArray == null) {
                return;
            }

            if (stringOrArray is string) {
                //sometimes component is sending a simple string straight
                commLog.SetAttribute(propertyName, stringOrArray);
                return;
            }
            if (!(stringOrArray is Array) || ((Array)stringOrArray).Length == 0) {
                //strange bug whereas the component was passing a blank array rather than either an array or a string
                //happens after selecting and deleting an item on screen
                return;
            }

            var sendToArray = ((IEnumerable)stringOrArray).Cast<object>()
                .Select(x => x.ToString())
                .ToArray();
            commLog.SetAttribute(propertyName, sendToArray.Length > 1 ? string.Join(",", sendToArray) : sendToArray[0]);
        }

        private void HandleAttachments(IEnumerable<AttachmentDTO> attachments, [NotNull] object maximoObj, [NotNull] ApplicationMetadata applicationMetadata) {
            if (maximoObj == null) throw new ArgumentNullException("maximoObj");
            if (applicationMetadata == null) throw new ArgumentNullException("applicationMetadata");

            attachments.ForEach(attachment => _attachmentHandler.AddAttachment(maximoObj, attachment));
        }

        private EmailData GenerateEmailObject(object integrationObject, IEnumerable<AttachmentDTO> attachments) {
            var emailAttachments = attachments.Select(dto => new EmailAttachment() {
                AttachmentName = dto.Path,
                AttachmentData = dto.Data,
                AttachmentBinary = dto.BinaryData
            }).ToList();

            return new EmailData(
                w.GetRealValue<string>(integrationObject, Sendfrom),
                w.GetRealValue<string>(integrationObject, Sendto),
                w.GetRealValue<string>(integrationObject, Subject),
                w.GetRealValue<string>(integrationObject, Message), emailAttachments) {
                Cc = w.GetRealValue<string>(integrationObject, Cc),
                BCc = w.GetRealValue<string>(integrationObject, Bcc)
            };
        }

        private List<AttachmentDTO> GetAttachments(CrudOperationData commlog, ApplicationMetadata appMetadata, CrudOperationData entity) {
            var attachmentData = commlog.GetUnMappedAttribute("attachment");
            var attachmentPath = commlog.GetUnMappedAttribute("newattachment_path");
            var attachments = _attachmentHandler.BuildAttachments(attachmentPath, attachmentData);

            var detailsHtml = commlog.GetUnMappedAttribute("detailsHtml");
            if (string.IsNullOrEmpty(detailsHtml)) {
                return attachments;
            }

            attachments.Add(CreateDetailsAttachment(detailsHtml, appMetadata, entity));
            return attachments;
        }

        private AttachmentDTO CreateDetailsAttachment(string detailsHtml, ApplicationMetadata appMetadata, CrudOperationData entity) {
            var fullContext = _lookuper.GetFromMemoryContext<SwHttpContext>("httpcontext");
            var appContext = fullContext.Context;
            var seccondBarIndex = appContext.IndexOf("/", 1, StringComparison.Ordinal);
            if (seccondBarIndex > 0) {
                appContext = appContext.Substring(0, appContext.IndexOf("/", 1, StringComparison.Ordinal));
            }

            if (Log.IsDebugEnabled) {
                Log.DebugFormat("Details Commlog original App context: " + appContext);
            }

            // appcontext not found on html, probably appcontext is "/"
            // or appcontext found as the content folder also probably appcontext is "/"
            if (detailsHtml.IndexOf("<link href=\"" + appContext, StringComparison.Ordinal) < 0 || "/Content".Equals(appContext)) {
                appContext = "/";
            }

            var serverPath = AppDomain.CurrentDomain.BaseDirectory;

            // make css paths absolute
            var processedHtml = detailsHtml.Replace("<link href=\"" + appContext, "<link href=\"" + serverPath);
            // make images paths absolute
            processedHtml = processedHtml.Replace(" src=\"" + appContext, " src=\"" + serverPath);

            if (Log.IsDebugEnabled) {
                Log.DebugFormat("Details Commlog App context: " + appContext);
                Log.DebugFormat("Details Commlog Server path: " + serverPath);
                Log.DebugFormat(processedHtml);
            }

            // limits title to 20 chars
            var fullTitle = appMetadata.Title + " Details";
            var title = fullTitle;
            if (title.Length > 20) {
                title = "";
                appMetadata.Title.Split(' ').ForEach(token => title += token.Substring(0, 1).ToUpper());
                title += " Details";
            }
            var hasUserId = !string.IsNullOrEmpty(entity.UserId);
            var description = fullTitle + (hasUserId ? " #" + entity.UserId : "");
            var path = appMetadata.Title + (hasUserId ? "(" + entity.UserId + ")" : "") + ".pdf";
            path = path.Replace(' ', '_');

            var dto = new AttachmentDTO() {
                Path = path,
                BinaryData = _pdfService.HtmlToPdf(processedHtml, fullTitle),
                Title = title,
                Description = description
            };

            //Comment to generate files for testing
            //File.WriteAllBytes(@"C:\temp\test.pdf", dto.BinaryData);
            //File.WriteAllText(@"C:\temp\test.html", processedHtml);

            return dto;
        }

        private async void UpdateEmailHistoryAsync(string userId, string[] emailAddresses) {
            await Task.Factory.NewThread(() => {
                string[] userIds = { userId.ToLower() };
                var emailRecords = _dao.FindByQuery<EmailHistory>(EmailHistory.byUserIdEmailAddess, userIds, emailAddresses).ToList();

                var newRecords = emailAddresses
                    .Where(address => !emailRecords.Any(email => email.EmailAddress.EqualsIc(address)))
                    .Select(address => new EmailHistory(null, userId, address))
                    .ToList();

                _dao.BulkSave(newRecords);
            });
        }

    }
}
