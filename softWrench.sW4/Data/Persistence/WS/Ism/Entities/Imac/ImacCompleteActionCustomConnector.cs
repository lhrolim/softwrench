﻿using System.Collections.Generic;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.Change;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Util;
using System;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.Imac {
    class ImacCompleteActionCustomConnector : IsmImacCrudConnectorDecorator {
        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeUpdate(maximoTemplateData);
            var jsonObject = (CrudOperationData)maximoTemplateData.OperationData;
            var serviceIncident = (ServiceIncident)maximoTemplateData.IntegrationObject;

            var taskId = jsonObject.GetUnMappedAttribute("WoActivityId");
            var activity = new Activity {
                type = "WOActivity",
                ActionID = taskId,
                ActionLogSummary = jsonObject.GetUnMappedAttribute("#tasksummary")
            };
            var fail = jsonObject.ContainsAttribute("#selectedAction") && "FAIL".EqualsIc(jsonObject.GetAttribute("#selectedAction") as string);

            var ownergroup = jsonObject.GetUnMappedAttribute("activityownergroup");
            var sequence = jsonObject.GetUnMappedAttribute("activitysequence");
            activity.FlexFields = ArrayUtil.PushRange(activity.FlexFields, BuildFlexFields(ownergroup, sequence, !fail));
            serviceIncident.Activity = ArrayUtil.Push(serviceIncident.Activity, activity);

            if (fail) {
                serviceIncident.Activity = PushExtraActivitiesForFailure(serviceIncident.Activity, jsonObject);
                var user = SecurityFacade.CurrentUser();
                var wlActivity = new Activity {
                    ActionLogSummary = "Task {0} Failed".Fmt(taskId),
                    ActionLog = jsonObject.GetUnMappedAttribute("#reasonreject"),
                    type = "WorkLog",
                    UserID = ISMConstants.AddEmailIfNeeded(user.MaximoPersonId),
                    ActivityType = "CLIENTNOTE"
                };

                serviceIncident.Activity = ArrayUtil.Push(serviceIncident.Activity, wlActivity);




            }

            CheckIMACResolved(serviceIncident, jsonObject);
        }

        //HAP-1170
        private Activity[] PushExtraActivitiesForFailure(Activity[] serviceIncidentActivity, CrudOperationData jsonObject) {
            var activity = new Activity {
                type = "WOActivity",
                ActionLogSummary = "Check Customer Input"
            };
            var sequence = int.Parse(jsonObject.GetUnMappedAttribute("activitysequence"));


            var flexFields = new List<FlexFieldsFlexField>{
                new FlexFieldsFlexField{
                    id = "0",
                    mappedTo = "STATUS",
                    Value = "WAPPR",
                },
                new FlexFieldsFlexField{
                    id = "0",
                    mappedTo = "WOSEQUENCE",
                    Value = (sequence + 1).ToString()
                },
                new FlexFieldsFlexField{
                    id = "0",
                    mappedTo = "OWNERGROUP",
                    Value = "I-EUS-DE-CSC-IMC-HLCIMAC"
                }
            };
            //
            activity.FlexFields = ArrayUtil.PushRange(activity.FlexFields, flexFields);
            serviceIncidentActivity = ArrayUtil.Push(serviceIncidentActivity, activity);


            activity = new Activity {
                type = "WOActivity",
                ActionLogSummary = jsonObject.GetUnMappedAttribute("#tasksummary")
            };

            flexFields = new List<FlexFieldsFlexField>{
                new FlexFieldsFlexField{
                    id = "0",
                    mappedTo = "STATUS",
                    Value = "WAPPR",
                },
                new FlexFieldsFlexField{
                    id = "0",
                    mappedTo = "WOSEQUENCE",
                    Value = (sequence + 2).ToString()
                },
                new FlexFieldsFlexField{
                    id = "0",
                    mappedTo = "OWNERGROUP",
                    Value = "C-HLC-WW-ITCALL"
                }
            };
            //
            activity.FlexFields = ArrayUtil.PushRange(activity.FlexFields, flexFields);
            return ArrayUtil.Push(serviceIncidentActivity, activity);

        }

        private static List<FlexFieldsFlexField> BuildFlexFields(string ownergroup, string sequence, bool completed) {
            var statusVal = completed ? "COMP" : "FAIL";

            var flexFields = new List<FlexFieldsFlexField>();
            flexFields.Add(new FlexFieldsFlexField {
                id = "0",
                mappedTo = "STATUS",
                Value = statusVal,
            });
            //
            flexFields.Add(new FlexFieldsFlexField {
                id = "0",
                mappedTo = "WOSEQUENCE",
                Value = sequence
            });

            flexFields.Add(new FlexFieldsFlexField {
                id = "0",
                mappedTo = "OWNERGROUP",
                Value = ownergroup
            });
            return flexFields;
        }

        private static void CheckIMACResolved(ServiceIncident serviceIncident, CrudOperationData jsonObject) {

            var action = jsonObject.GetUnMappedAttribute("#selectedAction");
            if ("COMP".Equals(action)) {

                var curSequence = Int32.Parse(jsonObject.GetUnMappedAttribute("activitysequence"));
                var maxSequence = 0;
                var activities = jsonObject.AssociationAttributes["woactivity_"] as IList<CrudOperationData>;
                foreach (var activity in activities) {
                    var sequence = (int)activity.GetAttribute("wosequence");
                    if (sequence > maxSequence) {
                        maxSequence = sequence;
                    }
                }

                // this means that the user is completing the last activity (max sequence) in the Job Plan list..
                if (curSequence == maxSequence) {
                    // .. so, resolve the IMAC as well
                    serviceIncident.WorkflowStatus = "RESOLVED";
                }
            }
        }


    }
}
