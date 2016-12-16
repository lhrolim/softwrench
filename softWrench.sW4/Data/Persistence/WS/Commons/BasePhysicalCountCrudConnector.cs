using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    class BasePhysicalCountCrudConnector : CrudConnectorDecorator {


        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var physicalCount = maximoTemplateData.IntegrationObject;
            w.SetValue(physicalCount, "reconciled", false);
            base.BeforeCreation(maximoTemplateData);
        }

        public override string ApplicationName() {
            return "physicalcount";
        }
    }
}
