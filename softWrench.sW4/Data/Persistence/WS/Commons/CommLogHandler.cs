using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Email;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using softWrench.sW4.wsWorkorder;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class CommLogHandler {

        private static BaseHibernateDAO _dao = new MaximoHibernateDAO();
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
            var newCommLogs = commlogs.Where(r => r.GetAttribute("commloguid") == null);
            var ownerid = w.GetRealValue(rootObject, ticketuid);
            w.CloneArray(newCommLogs, rootObject, "COMMLOG", delegate(object integrationObject, CrudOperationData crudData) {
                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.Add.ToString());
                var id = _dao.FindSingleByNativeQuery<object>("Select MAX(commlog.commlogid) from commlog", null);
                var rnd = new Random();
                var commlogid = Convert.ToInt32(id) + rnd.Next(1, 10);
                w.SetValue(integrationObject, Commlogid, commlogid);
                w.SetValue(integrationObject,Ownerid, ownerid);
                w.SetValueIfNull(integrationObject, ownertable, entity.TableName);
                w.SetValueIfNull(integrationObject, inbound, false);
                w.CopyFromRootEntity(rootObject, integrationObject, siteid, user.SiteId);
                w.CopyFromRootEntity(rootObject, integrationObject, orgid, user.OrgId);
                w.CopyFromRootEntity(rootObject, integrationObject, createby, user.Login, "CHANGEBY");
                w.CopyFromRootEntity(rootObject, integrationObject, createdate, DateTime.Now.FromServerToRightKind());
                w.CopyFromRootEntity(rootObject, integrationObject, modifydate, DateTime.Now.FromServerToRightKind());
                w.SetValueIfNull(integrationObject, "logtype", "CLIENTNOTE");
                LongDescriptionHandler.HandleLongDescription(integrationObject, crudData);
                HandleAttachments(entity, commlogs, maximoTemplateData.ApplicationMetadata);
                if (w.GetRealValue(integrationObject, sendto) != null) {
                    maximoTemplateData.Properties.Add("mailObject", GenerateEmailObject(integrationObject));
                }
            });
        }

        private static void HandleAttachments(CrudOperationData data, IEnumerable<CrudOperationData> maximoObj,
            Metadata.Applications.ApplicationMetadata applicationMetadata)
        {
            // Check if Attachment is present
            var attachmentData = data.GetUnMappedAttribute("newattachment");
            var attachmentPath = data.GetUnMappedAttribute("newattachment_path");

            if (!String.IsNullOrWhiteSpace(attachmentData) && !String.IsNullOrWhiteSpace(attachmentPath))
            {
                var user = SecurityFacade.CurrentUser();
                if (String.IsNullOrEmpty(attachmentData))
                {
                    return;
                }
                var docLink = ReflectionUtil.InstantiateSingleElementFromArray(maximoObj, "DOCLINKS");
                w.SetValue(docLink, "ADDINFO", true);
                w.CopyFromRootEntity(maximoObj, docLink, "CREATEBY", user.Login, "reportedby");
                w.CopyFromRootEntity(maximoObj, docLink, "CREATEDATE", DateTime.Now.FromServerToRightKind());
                w.CopyFromRootEntity(maximoObj, docLink, "CHANGEBY", user.Login, "reportedby");
                w.CopyFromRootEntity(maximoObj, docLink, "CHANGEDATE", DateTime.Now.FromServerToRightKind());
                w.CopyFromRootEntity(maximoObj, docLink, "SITEID", user.SiteId);
                w.CopyFromRootEntity(maximoObj, docLink, "ORGID", user.OrgId);
                w.SetValue(docLink, "URLTYPE", "FILE");
                w.SetValue(docLink, "URLNAME", attachmentPath);
                w.SetValue(docLink, "UPLOAD", true);
                w.SetValue(docLink, "DOCTYPE", "Attachments");
                w.SetValue(docLink, "DOCUMENT", FileUtils.GetNameFromPath(attachmentPath, 100));
                w.SetValue(docLink, "DESCRIPTION", attachmentPath);
                w.SetValue(docLink, "DOCUMENTDATA", FileUtils.ToByteArrayFromHtmlString(attachmentData));
            }
        }

        private static EmailService.EmailData GenerateEmailObject(object integrationObject)
        {
            return new EmailService.EmailData(w.GetRealValue<string>(integrationObject, sendfrom),
                w.GetRealValue<string>(integrationObject, sendto),
                w.GetRealValue<string>(integrationObject, subject),
                w.GetRealValue<string>(integrationObject, message)) {
                    Cc = w.GetRealValue<string>(integrationObject, cc)
                };
            
        }
    }
}
