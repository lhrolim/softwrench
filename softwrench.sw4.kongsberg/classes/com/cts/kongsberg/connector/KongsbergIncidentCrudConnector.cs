using softwrench.sw4.api.classes.email;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.connector {
    public class KongsbergIncidentCrudConnector : BaseIncidentCrudConnector {

        
        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var incident = maximoTemplateData.IntegrationObject;
            var sr = maximoTemplateData.IntegrationObject;
            var user = SecurityFacade.CurrentUser();

            HandleActualDates(sr);

            // TODO: Temp fix for getting change by to update with the userid. 
            // This workaround required trigger in the Maximo DB and custom attribute "SWCHANGEBY" in ticket
            w.SetValue(incident, "SWCHANGEBY", user.Login);
            //Handle Commlogs
            //Handle Commlogs
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            CommlogHandler.HandleCommLogs(maximoTemplateData, crudData, sr);


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
