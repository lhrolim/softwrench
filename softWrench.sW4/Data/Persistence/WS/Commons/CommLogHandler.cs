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

        public static void HandleCommLogs(MaximoOperationExecutionContext maximoTemplateData, CrudOperationData entity, object rootObject) {
            var user = SecurityFacade.CurrentUser();
            var commlogs = (IEnumerable<CrudOperationData>)entity.GetRelationship("commlog");
            var newCommLogs = commlogs.Where(r => r.GetAttribute("commloguid") == null);
            var ownerid = w.GetRealValue(rootObject, "ticketuid");
            w.CloneArray(newCommLogs, rootObject, "COMMLOG", delegate(object integrationObject, CrudOperationData crudData) {
                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.Add.ToString());
                var id = _dao.FindSingleByNativeQuery<object>("Select MAX(commlog.commlogid) from commlog", null);
                var rnd = new Random();
                var commlogid = Convert.ToInt32(id) + rnd.Next(1, 10);
                w.SetValue(integrationObject, "commlogid", commlogid);
                w.SetValue(integrationObject, "ownerid", ownerid);
                w.SetValueIfNull(integrationObject, "ownertable", entity.TableName);
                w.SetValueIfNull(integrationObject, "inbound", false);
                w.CopyFromRootEntity(rootObject, integrationObject, "siteid", user.SiteId);
                w.CopyFromRootEntity(rootObject, integrationObject, "orgid", user.OrgId);
                w.CopyFromRootEntity(rootObject, integrationObject, "createby", user.Login, "CHANGEBY");
                w.CopyFromRootEntity(rootObject, integrationObject, "createdate", DateTime.Now.FromServerToRightKind());
                w.CopyFromRootEntity(rootObject, integrationObject, "modifydate", DateTime.Now.FromServerToRightKind());
                w.SetValueIfNull(integrationObject, "logtype", "CLIENTNOTE");
                LongDescriptionHandler.HandleLongDescription(integrationObject, crudData);
                if (w.GetRealValue(integrationObject, "sentto") != null) {
                    maximoTemplateData.Properties.Add("mailObject", GenerateEmailObject(integrationObject));
                }
            });
        }

        private static EmailService.EmailData GenerateEmailObject(object integrationObject)
        {
            return new EmailService.EmailData(w.GetRealValue<string>(integrationObject, "sendfrom"),
                w.GetRealValue<string>(integrationObject, "sendto"),
                w.GetRealValue<string>(integrationObject, "subject"),
                w.GetRealValue<string>(integrationObject, "message")) {
                    Cc = w.GetRealValue<string>(integrationObject, "cc")
                };
            
        }
    }
}
