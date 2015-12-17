using System;
using System.Linq;
using JetBrains.Annotations;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.Imac {
    class IsmImacCrudConnectorDecorator : BaseISMTicketDecorator {

        private readonly IImacServicePlanHelper _servicePlanHelper;

        public IsmImacCrudConnectorDecorator() {
            _servicePlanHelper = SimpleInjectorGenericFactory.Instance.GetObject<IImacServicePlanHelper>(typeof(IImacServicePlanHelper));
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeCreation(maximoTemplateData);
            var jsonObject = (CrudOperationData)maximoTemplateData.OperationData;
            var webServiceObject = (ServiceIncident)maximoTemplateData.IntegrationObject;
            PopulateAssets(jsonObject, webServiceObject, maximoTemplateData.ApplicationMetadata.Schema.SchemaId);
            SetClassification(jsonObject, webServiceObject, maximoTemplateData.ApplicationMetadata.Schema.SchemaId);
            HandleBusinessMatrix(webServiceObject, jsonObject, maximoTemplateData.ApplicationMetadata.Schema.SchemaId);
            ISMAttachmentHandler.HandleAttachmentsForCreation(jsonObject, webServiceObject);
            var schemaId = maximoTemplateData.OperationData.ApplicationMetadata.Schema.SchemaId;
        }

        public override void AfterCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.AfterCreation(maximoTemplateData);
            var id = maximoTemplateData.ResultObject.Id;
            var schemaId = maximoTemplateData.OperationData.ApplicationMetadata.Schema.SchemaId;
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeUpdate(maximoTemplateData);
            var jsonObject = (CrudOperationData)maximoTemplateData.OperationData;
            var webServiceObject = (ServiceIncident)maximoTemplateData.IntegrationObject;
            var classificationId = jsonObject.GetAttribute("classificationid") as string;
            ISMAttachmentHandler.HandleAttachmentsForUpdate(jsonObject, webServiceObject);
            webServiceObject.Problem.System = classificationId;
        }


        private void HandleBusinessMatrix(ServiceIncident imac, CrudOperationData jsonObject, string schemaid) {
            var activities = _servicePlanHelper.LoadFromServicePlan(schemaid, jsonObject);
            if (activities != null) {
                imac.Activity = activities.ToArray();
            }
        }


        private static void PopulateAssets([NotNull] CrudOperationData entity, ServiceIncident maximoTicket, string schemaId) {
            var assetId = entity.GetAttribute("asset") as string;
            if (assetId == null || IsDecommissionTemplate(schemaId)) {
                //HAP-878 decommission assets cannot be applied here
                return;
            }
            var assetList = CollectionUtil.SingleElement(new Asset { AssetID = assetId });
            maximoTicket.Asset = assetList.ToArray();
        }

        private static bool IsDecommissionTemplate(string schemaId) {
            return "decommission".Equals(schemaId);
        }

        protected override ISMServiceEntities.Problem PopulateProblem(CrudOperationData jsonObject, ServiceIncident webServiceObject,
            string entityName, Boolean update, string schemaId) {
            var problem = base.PopulateProblem(jsonObject, webServiceObject, entityName, update, schemaId);
            var attribute = jsonObject.GetAttribute("fromlocation") as string;
            if (attribute != null && !attribute.StartsWith("HLC-DE") && attribute.Length == 3) {
                attribute = "HLC-DE-" + attribute;
            }
            problem.CustomerID = attribute;
            return problem;
        }

        protected override string HandleTitle(ServiceIncident webServiceObject, CrudOperationData entity, string schemaId) {
            if (IsDecommissionTemplate(schemaId)) {
                var assetId = entity.GetAttribute("asset") as string;
                return "Decommission of {0}".Fmt(assetId);
            }
            return entity.GetAttribute("titleoforder") as string;
        }

        protected override void HandleLongDescription(ServiceIncident webServiceObject, CrudOperationData entity, ApplicationMetadata metadata, bool update) {
            base.HandleLongDescription(webServiceObject, entity, metadata, update);
            if (!update) {
                webServiceObject.Problem.Description = ImacDescriptionHandler.BuildDescription(entity, metadata);
                if (metadata.Schema.SchemaId.StartsWith("install")) {
                    var costcenter = entity.GetAttribute("costcenter") as string;
                    if (costcenter != null) {
                        webServiceObject.Problem.Abstract += ("//" + costcenter.Split('/')[1]);
                    }
                }
            } else {
                webServiceObject.Problem.Abstract = entity.GetAttribute("description") as string;
            }


        }

        protected override string GetAffectedPerson(CrudOperationData entity) {
            return (string)entity.GetAttribute("primaryuser_.personid");
        }

        protected override string GetOverridenOwnerGroup(bool isCreation, CrudOperationData jsonObject) {
            return null;
        }

        private static void SetClassification(CrudOperationData jsonObject, ServiceIncident imac, string schemaid) {
            if ("installlan".Equals(schemaid) || "installother".Equals(schemaid) || "installstd".Equals(schemaid)) {
                imac.Problem.System = "81515000";
            } else if ("move".Equals(schemaid)) {
                imac.Problem.System = "81515100";
            } else if ("update".Equals(schemaid)) {
                imac.Problem.System = "81515300";
            } else if ("add".Equals(schemaid)) {
                imac.Problem.System = "81515200";
            } else if ("removestd".Contains(schemaid) || "removelan".Contains(schemaid) || "removeother".Contains(schemaid)) {
                imac.Problem.System = "81515400";
            } else if ("replacestd".Contains(schemaid) || "replacelan".Contains(schemaid) || "replaceother".Contains(schemaid)) {
                imac.Problem.System = "81515500";
            } else if ("decommission".Equals(schemaid)) {
                imac.Problem.System = "81515700";
            }
            jsonObject.Attributes["classification"] = imac.Problem.System;
        }
    }
}
