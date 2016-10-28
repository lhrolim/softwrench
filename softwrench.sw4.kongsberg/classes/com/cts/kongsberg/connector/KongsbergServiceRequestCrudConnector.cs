using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.connector
{
    public class KongsbergServiceRequestCrudConnector: BaseServiceRequestCrudConnector {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);

            HandleActualDates(maximoTemplateData);
            SetSwChangeBy(sr);

            base.BeforeUpdate(maximoTemplateData);
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;
            SetSwChangeBy(sr);
            base.BeforeCreation(maximoTemplateData);
        }
    }
}
