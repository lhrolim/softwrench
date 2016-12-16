using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using softwrench.sw4.Hapag.Data.WS.Ism.Base;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.Change {
    class ChangeApprovalActionCustomConnector : IsmChangeCrudConnector {
        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var jsonObject = (CrudOperationData)maximoTemplateData.OperationData;
            var changeRequest = (ChangeRequest)maximoTemplateData.IntegrationObject;
            var selectedaction = jsonObject.GetAttribute("#selectedAction") as string;
            var groupaction = jsonObject.GetAttribute("#groupAction") as string;
            ChangeApprovalsHandler(changeRequest, selectedaction, groupaction);
            // add worklog beforewards,so that @@ gets appended in the beggining
            base.BeforeUpdate(maximoTemplateData);
        }

        private static void ChangeApprovalsHandler(ChangeRequest changeRequest, string selectedaction, string groupName) {
            string log;
            string actionid;
            if (selectedaction == "Approved") {
                log = "Approved by group " + groupName;
                actionid = "APPROVAL OBTAINED";
            } else {
                log = "Rejected by group " + groupName;
                actionid = "REASON REJECTING";
            }
            var worklogList = new List<ChangeLog>();
            var user = SecurityFacade.CurrentUser();
            var changeLog = new ChangeLog {
                Log = log,
                ActionID = actionid,
                UserID = ISMConstants.AddEmailIfNeeded(user.MaximoPersonId),
                LogDateTimeSpecified = true,
                LogDateTime = DateTime.Now
            };
            worklogList.Add(changeLog);
            changeRequest.ChangeLog = ArrayUtil.PushRange(changeRequest.ChangeLog, worklogList);
        }
    }
}
