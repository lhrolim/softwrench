using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Util;
using System;
using softwrench.sw4.Hapag.Data.WS.Handlers;
using softwrench.sw4.Hapag.Data.WS.Ism.Base;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.SR {
    class IsmSRCrudConnectorDecorator : BaseISMTicketDecorator {


        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeCreation(maximoTemplateData);
            var jsonObject = (CrudOperationData)maximoTemplateData.OperationData;
            var webServiceObject = (ServiceIncident)maximoTemplateData.IntegrationObject;
            PopulateAssets(jsonObject, webServiceObject);
            ISMAttachmentHandler.HandleAttachmentsForCreation(jsonObject, webServiceObject);
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeUpdate(maximoTemplateData);
            var jsonObject = (CrudOperationData)maximoTemplateData.OperationData;
            var webServiceObject = (ServiceIncident)maximoTemplateData.IntegrationObject;
            HandleStatus(jsonObject, webServiceObject);
            ISMAttachmentHandler.HandleAttachmentsForUpdate(jsonObject, webServiceObject);
        }

        public override string ApplicationName() {
            return "servicerequest";
        }

        protected override Metrics PopulateMetrics(ServiceIncident webServiceObject, CrudOperationData jsonObject) {
            var metrics = base.PopulateMetrics(webServiceObject, jsonObject);
            metrics.ProblemOccurredDateTime = (DateTime)jsonObject.GetAttribute("affecteddate");
            return metrics;
        }

        private static void HandleStatus(CrudOperationData jsonObject, ServiceIncident webServiceObject) {
            var status = (string)jsonObject.GetAttribute("status");
            if ("SLAHOLD".Equals(status, StringComparison.CurrentCultureIgnoreCase)) {
                var nullOwner = string.IsNullOrEmpty((string)jsonObject.GetAttribute("owner"));
                status = nullOwner ? "QUEUED" : "INPROG";
            }
            webServiceObject.WorkflowStatus = status;

        }


        private static void PopulateAssets(CrudOperationData entity, ServiceIncident maximoTicket) {
            var assetId = entity.GetAttribute("assetnum") as string;
            if (assetId == null) {
                //for general and outlook templates, there might be only an itc asset selected
                //lets fallback to this
                assetId = entity.GetAttribute("itcassetnum") as string;
            }
            if (assetId == null) {
                return;
            }
            var assetList = CollectionUtil.SingleElement(new Asset { AssetID = assetId });
            maximoTicket.Asset = assetList.ToArray();
        }


        protected override ISMServiceEntities.Problem PopulateProblem(CrudOperationData jsonObject, ServiceIncident webServiceObject,
            string entityName, Boolean update) {
            var problem = base.PopulateProblem(jsonObject, webServiceObject, entityName, update);

            problem.CustomerID = jsonObject.GetAttribute("person_.pluspcustomer") as string;
            return problem;
        }






        protected override void HandleLongDescription(ServiceIncident webServiceObject, CrudOperationData entity, ApplicationMetadata metadata, bool update) {
            var problem = webServiceObject.Problem;
            var ld = (CrudOperationData)entity.GetRelationship("longdescription");
            if (ld != null) {
                var originalLd = (string)ld.GetAttribute("ldtext");
                if (!update) {
                    problem.Description = HapagSRLongDescriptionHandler.ParseSchemaBasedLongDescription(originalLd,
                        entity, metadata.Schema.SchemaId);
                } else {
                    problem.Description = originalLd;
                }
            }
        }

        protected override string GetAffectedPerson(CrudOperationData entity) {
            return (string)entity.GetAttribute("affectedperson");
        }
        protected override string GetTemplateId(CrudOperationData jsonObject) {
            return null;
        }
    }
}
