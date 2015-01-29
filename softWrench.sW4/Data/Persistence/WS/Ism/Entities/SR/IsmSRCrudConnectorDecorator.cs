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

            HapagChangeHandler.CheckSR4ChangeGroupID(jsonObject, webServiceObject);
        }

        protected override Metrics PopulateMetrics(ServiceIncident webServiceObject, CrudOperationData jsonObject) {
            var metrics = base.PopulateMetrics(webServiceObject, jsonObject);
            metrics.ProblemOccurredDateTime = (DateTime)jsonObject.GetAttribute("affecteddate");
            return metrics;
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

        // https://controltechnologysolutions.atlassian.net/browse/HAP-839
        protected override string GetOverridenOwnerGroup(bool isCreation, CrudOperationData jsonObject) {
            return HlagTicketUtil.HandleSRAndIncidentOwnerGroups(isCreation, jsonObject,ISMConstants.DefaultAssignedGroupSr);
        }
    }
}
