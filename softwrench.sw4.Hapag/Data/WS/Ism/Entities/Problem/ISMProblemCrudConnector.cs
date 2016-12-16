using softwrench.sw4.Hapag.Data.WS.Ism.Base;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.Problem {
    class IsmProblemCrudConnector : BaseISMTicketDecorator {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeUpdate(maximoTemplateData);
            var integrationObject = maximoTemplateData.IntegrationObject;
            ISMAttachmentHandler.HandleAttachmentsForUpdate((CrudOperationData)maximoTemplateData.OperationData, (ServiceIncident)integrationObject);
        }

        public override string ApplicationName() {
            return "problem";
        }


        protected override string GetProblemType() {
            return "PROBLEM";
        }

        protected override string GetAffectedPerson(CrudOperationData entity) {
            return (string)entity.GetAttribute("affectedperson");
        }


    }
}
