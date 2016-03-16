using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using WsUtil = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using softWrench.sW4.wsWorkorder;

namespace softWrench.sW4.Data.Persistence.WS.Commons
{

    class WorkLogHandler
    {

        public static void HandleWorkLogs(CrudOperationData entity, object rootObject)
        {
            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();

            // Workorder id used for data association
            var recordKey = entity.UserId;

            // Filter work order materials for any modified entries.  This is done by using the modifydate.  
            // Modifydate is null when detail schema is passed, which designate the record as updated or changed.  
            var Worklogs = ((IEnumerable<CrudOperationData>)entity.GetRelationship("worklog")).Where(w => w.UnmappedAttributes["#isDirty"] != null).ToArray();
            WsUtil.CloneArray(Worklogs, rootObject, "WORKLOG", delegate(object integrationObject, CrudOperationData crudData) {
                WsUtil.SetValueIfNull(integrationObject, "worklogid", -1);
                WsUtil.SetValue(integrationObject, "recordkey", recordKey);
                WsUtil.SetValueIfNull(integrationObject, "class", entity.TableName);
                WsUtil.SetValueIfNull(integrationObject, "createby", user.Login);
                WsUtil.SetValueIfNull(integrationObject, "logtype", "CLIENTNOTE");

                WsUtil.CopyFromRootEntity(rootObject, integrationObject, "siteid", user.SiteId);
                WsUtil.CopyFromRootEntity(rootObject, integrationObject, "orgid", user.OrgId);
                WsUtil.CopyFromRootEntity(rootObject, integrationObject, "createdate", DateTime.Now.FromServerToRightKind(), "CHANGEDATE");

                WsUtil.SetValue(integrationObject, "modifydate", DateTime.Now.FromServerToRightKind(), true);
                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.AddChange.ToString());
                LongDescriptionHandler.HandleLongDescription(integrationObject, crudData);
            });
        }
    }
}

