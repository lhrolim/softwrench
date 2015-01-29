using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.Incident {
    class IsmIncidentCrudConnector : BaseISMTicketDecorator {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeUpdate(maximoTemplateData);
            var jsonObject = (CrudOperationData)maximoTemplateData.OperationData;
            var webServiceObject = (ServiceIncident)maximoTemplateData.IntegrationObject;
            HandleStatus(jsonObject, webServiceObject);
            ISMAttachmentHandler.HandleAttachmentsForUpdate((CrudOperationData)maximoTemplateData.OperationData, webServiceObject);
        }


        protected override string GetProblemType() {
            return "INCIDENT";
        }

        protected override string GetAffectedPerson(CrudOperationData entity) {
            return (string)entity.GetAttribute("affectedperson");
        }

        protected override string GetOverridenOwnerGroup(bool isCreation, CrudOperationData jsonObject) {
            return HlagTicketUtil.HandleSRAndIncidentOwnerGroups(isCreation, jsonObject, ISMConstants.DefaultAssignedGroup);
        }

    }
}
