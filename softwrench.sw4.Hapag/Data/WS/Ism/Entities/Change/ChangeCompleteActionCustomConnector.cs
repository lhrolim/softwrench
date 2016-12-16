using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.Change {
    class ChangeCompleteActionCustomConnector : IsmChangeCrudConnector {
        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeUpdate(maximoTemplateData);
            var jsonObject = (CrudOperationData)maximoTemplateData.OperationData;
            var changeRequest = (ChangeRequest)maximoTemplateData.IntegrationObject;
            changeRequest.WorkOrder = new WorkOrder {
                Status = jsonObject.GetAttribute("#selectedAction") as string,
                WONum = jsonObject.GetAttribute("WoActivityId") as string
            };
        }


    }
}
