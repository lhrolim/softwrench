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

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeCreation(maximoTemplateData);
            var jsonObject = (CrudOperationData)maximoTemplateData.OperationData;
            var webServiceObject = (ServiceIncident)maximoTemplateData.IntegrationObject;
        }

        private Boolean IsWindowsTemplate(CrudOperationData data) {
            return "serverwindows".Equals(data.ApplicationMetadata.Schema.SchemaId);
        }



        protected override Metrics PopulateMetrics(ServiceIncident webServiceObject, CrudOperationData jsonObject) {
            var metrics = base.PopulateMetrics(webServiceObject, jsonObject);
            metrics.ProblemOccurredDateTime = (DateTime)jsonObject.GetAttribute("affecteddate");
            return metrics;
        }






        protected override ISMServiceEntities.Problem PopulateProblem(CrudOperationData jsonObject, ServiceIncident webServiceObject,
            string entityName, Boolean update, string schemaId) {
            var problem = base.PopulateProblem(jsonObject, webServiceObject, entityName, update, schemaId);

            var customer = jsonObject.GetAttribute("asset_.pluspcustomer") as string;
            if (customer == null) {
                //for general and outlook templates, there might be only an itc asset selected
                //lets fallback to this
                customer = jsonObject.GetAttribute("2asset_.pluspcustomer") as string;
            }
            if (customer == null) {
                //if no asset was selected, get the customer attribute from the Affected Person
                customer = jsonObject.GetAttribute("person_.pluspcustomer") as string;
            }
            problem.CustomerID = customer;

            return problem;
        }

        protected override string HandleTitle(ServiceIncident webServiceObject, CrudOperationData jsonObject, string schemaId) {
            var kfdc = "kfdc"+jsonObject.GetAttribute("hostname");
            if (IsWindowsTemplate(jsonObject)) {
                return "Offering new virtual Windows Server " + kfdc;
            }
            return "Offering new virtual Linux Server " + kfdc;
        }




        protected override void HandleLongDescription(ServiceIncident webServiceObject, CrudOperationData entity, ApplicationMetadata metadata, bool update) {
            var problem = webServiceObject.Problem;
            problem.Description = HapagOfferingLongDescriptionHandler.ParseSchemaBasedLongDescription(entity, metadata.Schema.SchemaId);
        }

        protected override string GetAffectedPerson(CrudOperationData entity) {
            return null;
        }
        protected override string GetTemplateId(CrudOperationData jsonObject) {
            return IsWindowsTemplate(jsonObject) ? "HLCDEOWIN" : "HLCDEOLNX";
        }

        // https://controltechnologysolutions.atlassian.net/browse/HAP-839
        protected override string GetOverridenOwnerGroup(bool isCreation, CrudOperationData jsonObject) {
            return "I-SM-DE-TAT-TPS-PROJECTMANAG";
        }
    }
}
