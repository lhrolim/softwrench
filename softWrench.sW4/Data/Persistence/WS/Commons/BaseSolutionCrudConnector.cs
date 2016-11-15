using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    public class BaseSolutionCrudConnector : CrudConnectorDecorator {
        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            CommonTransaction(maximoTemplateData);
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            CommonTransaction(maximoTemplateData);
        }

        public void CommonTransaction(MaximoOperationExecutionContext maximoTemplateData) {
            LongDescriptionHandler.HandleLongDescription(maximoTemplateData.IntegrationObject, (CrudOperationData)maximoTemplateData.OperationData, "PROBLEMCODE_LONGDESCRIPTION", "symptom");
            LongDescriptionHandler.HandleLongDescription(maximoTemplateData.IntegrationObject, (CrudOperationData)maximoTemplateData.OperationData, "FR1CODE_LONGDESCRIPTION", "cause");
            LongDescriptionHandler.HandleLongDescription(maximoTemplateData.IntegrationObject, (CrudOperationData)maximoTemplateData.OperationData, "FR2CODE_LONGDESCRIPTION", "resolution");
        }
    }
}
