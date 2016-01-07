using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;

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

        protected override void HandleDescription(CrudOperationData operationData, string description, ChangeRequest maximoTicket) {
            //https://controltechnologysolutions.atlassian.net/browse/HAP-993
            var selectedaction = operationData.GetAttribute("#selectedAction") as string;
            var lastAction = bool.Parse(operationData.GetAttribute("#lastaction") as string);
            var isApproved = "approved".EqualsIc(selectedaction);
            if (isApproved && !lastAction) {
                Log.Info("Approval but not the last no need to change summary");
                return;
            }
            var prefix = isApproved ? "@APR@" : "@REJ@";
            if (description.Length > 95) {
                //we need to make sure the size is never bigger than 100
                description = prefix + description.Substring(0, 95);
            } else {
                description = prefix + description;
            }
            maximoTicket.Change.Description = description;
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
