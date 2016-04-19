using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using NHibernate.Linq;
using softwrench.sw4.api.classes.email;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Email;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using softWrench.sW4.wsWorkorder;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Maximo;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class CommLogHandler {

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


        public CommLogHandler() {
            _dao = SimpleInjectorGenericFactory.Instance.GetObject<ISWDBHibernateDAO>(typeof(ISWDBHibernateDAO));
            _attachmentHandler = new AttachmentHandler();
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
                if(sendTo == null) throw new ArgumentNullException("To:");
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
                maximoTemplateData.Properties.Add("mailObject", GenerateEmailObject(integrationObject, crudData));
                HandleAttachments(crudData, integrationObject, maximoTemplateData.ApplicationMetadata);
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
            var allAddresses = (string) ccEmail != "" ? recipientEmail + "," + ccEmail : recipientEmail;
            allAddresses = (string) bccEmail != "" ? allAddresses + "," + bccEmail : allAddresses;
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

        private void HandleAttachments(CrudOperationData data, [NotNull] object maximoObj, [NotNull] Metadata.Applications.ApplicationMetadata applicationMetadata) {
            if (maximoObj == null) throw new ArgumentNullException("maximoObj");
            if (applicationMetadata == null) throw new ArgumentNullException("applicationMetadata");

            GetAttachments(data).ForEach(attachment => _attachmentHandler.AddAttachment(maximoObj, attachment));
        }

        private EmailData GenerateEmailObject(object integrationObject, CrudOperationData crudData) {
            var attachments = GetAttachments(crudData).Select(dto => new EmailAttachment(dto.Data, dto.Path)).ToList();

            return new EmailData(
                w.GetRealValue<string>(integrationObject, Sendfrom),
                w.GetRealValue<string>(integrationObject, Sendto),
                w.GetRealValue<string>(integrationObject, Subject),
                w.GetRealValue<string>(integrationObject, Message), attachments) {
                    Cc = w.GetRealValue<string>(integrationObject, Cc),
                    BCc = w.GetRealValue<string>(integrationObject, Bcc)
                };
        }

        private IEnumerable<AttachmentDTO> GetAttachments(CrudOperationData commlog) {
            var attachmentData = commlog.GetUnMappedAttribute("attachment");
            var attachmentPath = commlog.GetUnMappedAttribute("newattachment_path");
            return _attachmentHandler.BuildAttachments(attachmentPath, attachmentData);
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
