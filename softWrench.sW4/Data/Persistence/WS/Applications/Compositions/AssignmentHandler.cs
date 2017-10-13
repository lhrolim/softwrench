using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using Common.Logging;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.wsWorkorder;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {

    public class AssignmentHandler : ISingletonComponent{

        private static readonly ILog Log = LogManager.GetLogger(typeof(AssignmentHandler));


        public void CreateNewAssignmentForToday(object root, DateTime nowServer){
            var user = SecurityFacade.CurrentUser();
            var arr = ReflectionUtil.InstantiateArrayWithBlankElements(root, "ASSIGNMENT", 1);
            var assignment = arr.GetValue(0);

            WsUtil.SetValue(assignment, "LABORCODE", user.GetProperty("laborcode"));
            WsUtil.SetValue(assignment, "SCHEDULEDATE", nowServer);
            WsUtil.SetValue(assignment, "FINISHDATE", DateTime.Now.AddMonths(2).FromServerToRightKind());
            WsUtil.CopyFromRootEntity(root, assignment, "orgid", user.OrgId);
        }


        public void HandleAssignments(CrudOperationData entity, object rootObject) {
            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();

            // Workorder id used for data association
            var recordKey = entity.UserId;



            // SWWEB-2365: send only edited or new worklogs
            var assignments = ((IEnumerable<CrudOperationData>)entity.GetRelationship("assignment"))
                            .Where(w => w.UnmappedAttributes.ContainsKey("#isDirty"))
                            .ToArray();

            WsUtil.CloneArray(assignments, rootObject, "ASSIGNMENT", delegate (object assignment, CrudOperationData crudData) {
//                WsUtil.SetValueIfNull(assignment, "LABORCODE", user.GetProperty("laborcode"));
//                WsUtil.SetValueIfNull(assignment, "SCHEDULEDATE", nowServer);
                WsUtil.SetValue(assignment, "FINISHDATE", DateTime.Now.AddMonths(2).FromServerToRightKind());
            });
        }

    }
}
