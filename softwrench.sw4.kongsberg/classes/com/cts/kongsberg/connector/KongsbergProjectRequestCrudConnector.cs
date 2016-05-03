using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.connector
{
    public class KongsbergProjectRequestCrudConnector: KongsbergServiceRequestCrudConnector {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            TicketspecHandler.HandleTicketspec(maximoTemplateData);
            base.BeforeUpdate(maximoTemplateData);
        }

        public override void AfterCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.AfterUpdate(maximoTemplateData);
            maximoTemplateData.OperationData.UserId = maximoTemplateData.ResultObject.UserId;
            maximoTemplateData.OperationData.OperationType = softWrench.sW4.Data.Persistence.WS.Internal.OperationType.AddChange;

            // Resubmitting MIF for ServiceAddress Update
            ConnectorEngine.Update((CrudOperationData)maximoTemplateData.OperationData);
        }
    }
}
