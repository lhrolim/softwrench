using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using softWrench.sW4.wsWorkorder;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    class WorkLogHandler {
      
        public static void HandleWorkLogs(CrudOperationData entity, object rootObject)
        {
            var user = SecurityFacade.CurrentUser();
            var worklogs = (IEnumerable<CrudOperationData>)entity.GetRelationship("worklog");
            var newWorkLogs = worklogs.Where(r => r.GetAttribute("worklogid") == null);
//            var association =entity.EntityMetadata.Associations.First(a => a.To == "worklog");
            var recordKey = entity.Id;
            w.CloneArray(worklogs, rootObject, "WORKLOG", delegate(object integrationObject, CrudOperationData crudData) {

                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.AddChange.ToString());
                w.SetValueIfNull(integrationObject, "worklogid", -1);
                w.SetValue(integrationObject,"recordkey",recordKey);
                w.SetValueIfNull(integrationObject,"class",entity.TableName);
                
                w.CopyFromRootEntity(rootObject,integrationObject,"siteid",user.SiteId);
                w.CopyFromRootEntity(rootObject,integrationObject,"orgid",user.OrgId);
                w.CopyFromRootEntity(rootObject, integrationObject, "createby", user.Login, "CHANGEBY");
                w.CopyFromRootEntity(rootObject, integrationObject, "createdate", DateTime.Now.FromServerToRightKind());
                w.CopyFromRootEntity(rootObject, integrationObject, "modifydate", DateTime.Now.FromServerToRightKind());
                w.SetValueIfNull(integrationObject, "logtype", "CLIENTNOTE");

                LongDescriptionHandler.HandleLongDescription(integrationObject, crudData);
            });
        }
    }
}


