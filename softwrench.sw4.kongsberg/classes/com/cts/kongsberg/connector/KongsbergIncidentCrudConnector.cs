using softwrench.sw4.api.classes.email;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.connector {
    public class KongsbergIncidentCrudConnector : BaseIncidentCrudConnector {

        
        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var incident = maximoTemplateData.IntegrationObject;

            HandleActualDates(incident);
            SetSwChangeBy(incident);

            //Handle Commlogs
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            CommlogHandler.HandleCommLogs(maximoTemplateData, crudData, incident);

            base.BeforeUpdate(maximoTemplateData);
        }

        public override void AfterUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            if (maximoTemplateData.Properties.ContainsKey("mailObject")) {
                EmailService.SendEmailAsync((EmailData)maximoTemplateData.Properties["mailObject"]);
            }

            //TODO: Delete the failed commlog entry or marked as failed : Input from JB needed 
            base.AfterUpdate(maximoTemplateData);
        }
    }
}
