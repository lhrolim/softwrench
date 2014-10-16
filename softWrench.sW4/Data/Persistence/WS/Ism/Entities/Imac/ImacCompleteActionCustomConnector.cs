using System.Collections.Generic;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.Change;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Util;
using System;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.Imac {
    class ImacCompleteActionCustomConnector : IsmImacCrudConnectorDecorator {
        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeUpdate(maximoTemplateData);
            var jsonObject = (CrudOperationData)maximoTemplateData.OperationData;
            var serviceIncident = (ServiceIncident)maximoTemplateData.IntegrationObject;
            var activity = new Activity {
                type = "WOActivity",
                ActionID = jsonObject.GetUnMappedAttribute("WoActivityId"),
                ActionLogSummary = jsonObject.GetUnMappedAttribute("#tasksummary")
            };
            var ownergroup = jsonObject.GetUnMappedAttribute("activityownergroup") as string;
            var sequence = jsonObject.GetUnMappedAttribute("activitysequence") as string;
            activity.FlexFields = ArrayUtil.PushRange(activity.FlexFields, BuildFlexFields(ownergroup,sequence));
            serviceIncident.Activity = ArrayUtil.Push(serviceIncident.Activity, activity);
            CheckIMACResolved(serviceIncident, jsonObject);
        }

        private static List<FlexFieldsFlexField> BuildFlexFields(string ownergroup,string sequence) {
            var flexFields = new List<FlexFieldsFlexField>();
            flexFields.Add(new FlexFieldsFlexField() {
                id = "0",
                mappedTo = "STATUS",
                Value = "COMP"
            });
//
            flexFields.Add(new FlexFieldsFlexField() {
                id = "0",
                mappedTo = "WOSEQUENCE",
                Value = sequence
            });

            flexFields.Add(new FlexFieldsFlexField() {
                id = "0",
                mappedTo = "OWNERGROUP",
                Value = ownergroup
            });
            return flexFields;
        }

        private static void CheckIMACResolved(ServiceIncident serviceIncident, CrudOperationData jsonObject) {
            
            var action = jsonObject.GetUnMappedAttribute("#selectedAction") as string;
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
