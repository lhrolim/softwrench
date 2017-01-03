using System.EnterpriseServices;
using log4net;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Util;
using System;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.SR {
    class IsmOfferingCrudConnectorDecorator : BaseISMTicketDecorator {



        private Boolean IsWindowsTemplate(CrudOperationData data) {
            return "serverwindows".Equals(data.ApplicationMetadata.Schema.SchemaId);
        }

        protected override string HandleTitle(ServiceIncident webServiceObject, CrudOperationData jsonObject, string schemaId) {
            var kfdc = "kfdc" + jsonObject.GetAttribute("hostname");
            if (IsWindowsTemplate(jsonObject)) {
                return "Offering new virtual Windows Server " + kfdc;
            }
            return "Offering new virtual Linux Server " + kfdc;
        }


        protected override ISMServiceEntities.Problem PopulateProblem(CrudOperationData jsonObject, ServiceIncident webServiceObject,
       string entityName, Boolean update, string schemaId) {
            var problem = base.PopulateProblem(jsonObject, webServiceObject, entityName, update, schemaId);
            problem.CustomerID = "HLC-00";
            problem.System = "21390000";
            return problem;
        }

        protected override void HandleLongDescription(ServiceIncident webServiceObject, CrudOperationData entity, ApplicationMetadata metadata, bool update) {
            var problem = webServiceObject.Problem;
            problem.Description = HapagOfferingLongDescriptionHandler.ParseSchemaBasedLongDescription(entity);
        }

        protected override string GetAffectedPerson(CrudOperationData entity) {
            return null;
        }
        protected override string GetTemplateId(CrudOperationData jsonObject) {
            return IsWindowsTemplate(jsonObject) ? "HLCDEOWIN" : "HLCDEOLNX";
        }

        // https://controltechnologysolutions.atlassian.net/browse/HAP-839
        protected override string GetOverridenOwnerGroup(bool isCreation, CrudOperationData jsonObject) {
            return "I-EUS-DE-CSC-SDK-HLCFRONTDESKI";
        }
    }
}
