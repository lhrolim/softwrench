using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.connector
{
    public class KongsbergServiceRequestCrudConnector: BaseServiceRequestCrudConnector {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            TicketspecHandler.HandleTicketspec(maximoTemplateData);
            var sr = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);

            HandleActualDates(maximoTemplateData);
            SetSwChangeBy(sr);
            SetReloadAfterSave(maximoTemplateData);

            base.BeforeUpdate(maximoTemplateData);
        }

        public override void AfterCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.AfterCreation(maximoTemplateData);
            maximoTemplateData.OperationData.UserId = maximoTemplateData.ResultObject.UserId;
            maximoTemplateData.OperationData.OperationType = softWrench.sW4.Data.Persistence.WS.Internal.OperationType.AddChange;

            // Resubmitting MIF for ServiceAddress Update
            ConnectorEngine.Update((CrudOperationData)maximoTemplateData.OperationData);
        }


        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;
            SetSwChangeBy(sr);
            base.BeforeCreation(maximoTemplateData);
        }

        public override string ClientFilter() {
            return "kongsberg";
        }
    }
}
