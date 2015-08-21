using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Email;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using softWrench.sW4.wsWorkorder;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Maximo;
using softWrench.sW4.Data.Persistence.SWDB;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class CommLogHandler {

        private const string ticketuid = "ticketuid";
        private const string commloguid = "commloguid";
        private const string commlog = "commlog";
        private const string Commlogid = "commlogid";
        private const string Ownerid = "ownerid";
        private const string ownertable = "ownertable";
        private const string inbound = "inbound";
        private const string siteid = "siteid";
        private const string orgid = "orgid";
        private const string createby = "createby";
        private const string createdate = "createdate";
        private const string modifydate = "modifydate";
        private const string sendto = "sendto";
        private const string sendfrom = "sendfrom";
        private const string subject = "subject";
        private const string message = "message";
        private const string cc = "cc";

       
        public static void HandleCommLogs(MaximoOperationExecutionContext maximoTemplateData, CrudOperationData entity, object rootObject) {
            var user = SecurityFacade.CurrentUser();
            var commlogs = (IEnumerable<CrudOperationData>)entity.GetRelationship(commlog);
            var newCommLogs = commlogs.Where(r => r.GetAttribute(commloguid) == null);
            foreach (CrudOperationData commLog in commlogs) {
                // Convert sendto array to a comma separated list
                var sendtoObject = commLog.GetAttribute(sendto);
                var sendtoArray = ((IEnumerable) sendtoObject).Cast<object>()
                                                              .Select(x => x.ToString())
                                                              .ToArray();
                commLog.SetAttribute(sendto, string.Join(",", sendtoArray));
                // Convert cc array to a comma separated list
                var ccObject = commLog.GetAttribute(cc);
                if (ccObject != null) {
                    var ccArray = ((IEnumerable) ccObject).Cast<object>()
                        .Select(x => x.ToString())
                        .ToArray();
                    commLog.SetAttribute(cc, string.Join(",", ccArray));
                }
            }
            var ownerid = w.GetRealValue(rootObject, ticketuid);
            w.CloneArray(newCommLogs, rootObject, "COMMLOG", delegate(object integrationObject, CrudOperationData crudData) {
                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.Add.ToString());
                var id = MaximoHibernateDAO.GetInstance().FindSingleByNativeQuery<object>("Select MAX(commlog.commlogid) from commlog", null);
                var rnd = new Random();
                var commlogid = Convert.ToInt32(id) + rnd.Next(1, 10);
                w.SetValue(integrationObject, Commlogid, commlogid);
                w.SetValue(integrationObject, Ownerid, ownerid);
                w.SetValueIfNull(integrationObject, ownertable, entity.TableName);
                w.SetValueIfNull(integrationObject, inbound, false);
                w.CopyFromRootEntity(rootObject, integrationObject, siteid, user.SiteId);
                w.CopyFromRootEntity(rootObject, integrationObject, orgid, user.OrgId);
                w.CopyFromRootEntity(rootObject, integrationObject, createby, user.Login, "CHANGEBY");
                w.CopyFromRootEntity(rootObject, integrationObject, createdate, DateTime.Now.FromServerToRightKind(), "CHANGEDATE");
                w.CopyFromRootEntity(rootObject, integrationObject, modifydate, DateTime.Now.FromServerToRightKind());
                w.SetValueIfNull(integrationObject, "logtype", "CLIENTNOTE");
                LongDescriptionHandler.HandleLongDescription(integrationObject, crudData);
                HandleAttachments(crudData, rootObject, maximoTemplateData.ApplicationMetadata);
                if (w.GetRealValue(integrationObject, sendto) != null) {
                    maximoTemplateData.Properties.Add("mailObject", GenerateEmailObject(integrationObject, crudData));
                } else {
                    throw new System.ArgumentNullException("To:");
                }
                var recipientEmail = w.GetRealValue(integrationObject, sendto).ToString();
                var ccEmail = w.GetRealValue(integrationObject, cc);
                ccEmail = ccEmail != null ? ccEmail.ToString() : "";
                var allAddresses = ccEmail != "" ? recipientEmail + "," + ccEmail : recipientEmail;
                var username = user.MaximoPersonId;
                // TODO: Move this call off to a separate thread to speed up return time. User does not need to wait for the email addresses to be processed and stored.
                _updateEmailHistory(username, allAddresses.ToLower().Split(','));
            });
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

            if (!String.IsNullOrWhiteSpace(attachmentData) && !String.IsNullOrWhiteSpace(attachmentPath)) {
                var attachmentsData = data.GetUnMappedAttribute("attachment").Split(',');
                var attachmentsPath = attachmentPath.Split(',');
                AttachmentHandler attachment = new AttachmentHandler();
                for (int i = 0, j=0; i < attachmentsPath.Length; i++){
                    var content = new AttachmentDTO() {
                        Data = attachmentsData[j] + ',' + attachmentsData[j+1],
                        Path = attachmentPath.ToString()
                    };
                    attachment.AddAttachment(maximoObj, content);
                    j = j + 2;
                }

            }
        }

        private static EmailService.EmailData GenerateEmailObject(object integrationObject, CrudOperationData crudData)
        {
            List<EmailService.EmailAttachment> attachments = new List<EmailService.EmailAttachment>();
            if (!String.IsNullOrWhiteSpace(crudData.GetUnMappedAttribute("attachment")) &&
                !String.IsNullOrWhiteSpace(crudData.GetUnMappedAttribute("newattachment_path"))){
                var attachmentsData = crudData.GetUnMappedAttribute("attachment").Split(',');
                var attachmentsPath = crudData.GetUnMappedAttribute("newattachment_path").Split(',');
                for (int i = 0, j = 0; i < attachmentsPath.Length; i++){
                    EmailService.EmailAttachment attachment =
                        new EmailService.EmailAttachment(attachmentsData[j] + ',' + attachmentsData[j + 1],
                            attachmentsPath[i]);
                    attachments.Add(attachment);
                    j = j + 2;
                }
            }

            return new EmailService.EmailData(w.GetRealValue<string>(integrationObject, sendfrom),
                w.GetRealValue<string>(integrationObject, sendto),
                w.GetRealValue<string>(integrationObject, subject),
                w.GetRealValue<string>(integrationObject, message),
                attachments) {
                    Cc = w.GetRealValue<string>(integrationObject, cc)
                };

            
        }

        private static void _updateEmailHistory(string userId, string[] emailAddresses) {
            ISWDBHibernateDAO _swdbDao = SWDBHibernateDAO.GetInstance();
            string[] userIds = { userId.ToLower() };
            List<EmailHistory> emailRecords = _swdbDao.FindByQuery<EmailHistory>(EmailHistory.byUserIdEmailAddess, userIds, emailAddresses).ToList();
            List<EmailHistory> newRecords = new List<EmailHistory>();
            foreach (var emailAddress in emailAddresses) {
                if (!emailRecords.Any(t => t.EmailAddress.EqualsIc(emailAddress))) {
                    newRecords.Add(new EmailHistory(null, userId, emailAddress));
                }
            }
            _swdbDao.BulkSave(newRecords);
        }

   }
}
