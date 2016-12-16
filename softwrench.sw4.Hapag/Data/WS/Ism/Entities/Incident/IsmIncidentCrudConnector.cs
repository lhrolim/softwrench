using softwrench.sw4.Hapag.Data.WS.Ism.Base;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.Incident {
    class IsmIncidentCrudConnector : BaseISMTicketDecorator {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeUpdate(maximoTemplateData);
            var integrationObject = maximoTemplateData.IntegrationObject;
            ISMAttachmentHandler.HandleAttachmentsForUpdate((CrudOperationData)maximoTemplateData.OperationData, (ServiceIncident)integrationObject);
        }

        public override string ApplicationName() {
            return "incident";
        }


        protected override string GetProblemType() {
            return "INCIDENT";
        }

        protected override string GetAffectedPerson(CrudOperationData entity) {
            return (string)entity.GetAttribute("affectedperson");
        }


    }
}
