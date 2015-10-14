using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
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
using softWrench.sW4.Metadata.Security;

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


        private static ISWDBHibernateDAO _dao;

        public CommLogHandler() {
            _dao = SimpleInjectorGenericFactory.Instance.GetObject<ISWDBHibernateDAO>(typeof(ISWDBHibernateDAO));
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
                w.CopyFromRootEntity(rootObject, integrationObject, Createdate, DateTime.Now.FromServerToRightKind(), "CHANGEDATE");
                w.CopyFromRootEntity(rootObject, integrationObject, Modifydate, DateTime.Now.FromServerToRightKind());
                w.SetValueIfNull(integrationObject, "logtype", "CLIENTNOTE");
                LongDescriptionHandler.HandleLongDescription(integrationObject, crudData);
                HandleAttachments(crudData, rootObject, maximoTemplateData.ApplicationMetadata);
                if (w.GetRealValue(integrationObject, Sendto) != null) {
                    maximoTemplateData.Properties.Add("mailObject", GenerateEmailObject(integrationObject, crudData));
                } else {
                    throw new System.ArgumentNullException("To:");
                }
                var username = user.Login;
                var allAddresses = GetListOfAllAddressesUsed(integrationObject);
                // TODO: Move this call off to a separate thread to speed up return time. User does not need to wait for the email addresses to be processed and stored.
                _updateEmailHistory(username, allAddresses.ToLower().Split(','));
            });
        }

        private static string GetListOfAllAddressesUsed(object integrationObject) {
            var recipientEmail = w.GetRealValue(integrationObject, Sendto).ToString();
            var ccEmail = w.GetRealValue(integrationObject, Cc);
            var bccEmail = w.GetRealValue(integrationObject, Bcc);
            ccEmail = ccEmail != null ? ccEmail.ToString() : "";
            bccEmail = bccEmail != null ? bccEmail.ToString() : "";
            var allAddresses = ccEmail != "" ? recipientEmail + "," + ccEmail : recipientEmail;
            allAddresses = bccEmail != "" ? allAddresses + "," + bccEmail : allAddresses;
            return allAddresses;
        }

        private static void HandleArrayOfOptions(AttributeHolder commLog, string propertyName) {
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

        private static void HandleAttachments(CrudOperationData data, [NotNull] object maximoObj,
            [NotNull] Metadata.Applications.ApplicationMetadata applicationMetadata) {
            if (maximoObj == null)
                throw new ArgumentNullException("maximoObj");
            if (applicationMetadata == null)
                throw new ArgumentNullException("applicationMetadata");
            // Check if Attachment is present and make arrays of attachment paths and attachments 
            var attachmentData = data.GetUnMappedAttribute("attachment");
            var attachmentPath = data.GetUnMappedAttribute("newattachment_path");

            if (!string.IsNullOrWhiteSpace(attachmentData) && !string.IsNullOrWhiteSpace(attachmentPath)) {
                var attachmentsData = data.GetUnMappedAttribute("attachment").Split(',');
                var attachmentsPath = attachmentPath.Split(',');
                var attachment = new AttachmentHandler();
                for (int i = 0, j = 0; i < attachmentsPath.Length; i++) {
                    var content = new AttachmentDTO() {
                        Data = attachmentsData[j] + ',' + attachmentsData[j + 1],
                        Path = attachmentPath.ToString()
                    };
                    attachment.AddAttachment(maximoObj, content);
                    j = j + 2;
                }

            }
        }

        private EmailData GenerateEmailObject(object integrationObject, CrudOperationData crudData) {
            var attachments = new List<EmailAttachment>();
            if (!string.IsNullOrWhiteSpace(crudData.GetUnMappedAttribute("attachment")) &&
                !string.IsNullOrWhiteSpace(crudData.GetUnMappedAttribute("newattachment_path"))) {
                var attachmentsData = crudData.GetUnMappedAttribute("attachment").Split(',');
                var attachmentsPath = crudData.GetUnMappedAttribute("newattachment_path").Split(',');
                for (int i = 0, j = 0; i < attachmentsPath.Length; i++) {
                    var attachment = new EmailAttachment(attachmentsData[j] + ',' + attachmentsData[j + 1], attachmentsPath[i]);
                    attachments.Add(attachment);
                    j = j + 2;
                }
            }

            return new EmailData(w.GetRealValue<string>(integrationObject, Sendfrom),
                w.GetRealValue<string>(integrationObject, Sendto),
                w.GetRealValue<string>(integrationObject, Subject),
                w.GetRealValue<string>(integrationObject, Message), attachments) {
                Cc = w.GetRealValue<string>(integrationObject, Cc),
                BCc = w.GetRealValue<string>(integrationObject, Bcc)
            };


        }

        private void _updateEmailHistory(string userId, string[] emailAddresses) {

            string[] userIds = { userId.ToLower() };
            var emailRecords = _dao.FindByQuery<EmailHistory>(EmailHistory.byUserIdEmailAddess, userIds, emailAddresses).ToList();
            var newRecords = new List<EmailHistory>();
            foreach (var emailAddress in emailAddresses) {
                if (!emailRecords.Any(t => t.EmailAddress.EqualsIc(emailAddress))) {
                    newRecords.Add(new EmailHistory(null, userId, emailAddress));
                }
            }
            _dao.BulkSave(newRecords);
        }

    }
}
