using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;
using System;
using System.Linq;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.Change {
    class IsmNewChangeCrudConnectorDecorator : BaseISMTicketDecorator {

        protected IContextLookuper ContextManager {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<IContextLookuper>(typeof(IContextLookuper));
            }
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeCreation(maximoTemplateData);
            var jsonObject = (CrudOperationData)maximoTemplateData.OperationData;
            var webServiceObject = (ServiceIncident)maximoTemplateData.IntegrationObject;
            HandleFields(jsonObject, webServiceObject);
            HapagChangeHandler.FillDefaultValuesNewChange(webServiceObject);
            PopulateAssets(jsonObject, webServiceObject);
            ISMAttachmentHandler.HandleAttachmentsForCreation(jsonObject, webServiceObject);
        }

        private static void HandleFields(CrudOperationData jsonObject, ServiceIncident webServiceObject) {
            var priorityAux = (string)jsonObject.GetAttribute("#priority");
            int priority;
            if (int.TryParse(priorityAux, out priority)) {
                webServiceObject.ProviderPriority = priority;
            }
            webServiceObject.Problem.Abstract = (string)jsonObject.GetAttribute("description");
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
            //TODO: we have just one asset on the application, but they expect many... what´s wrong with that? --> child assets might be multiple, like on imacs.
            var assetId = entity.GetAttribute("assetnum") as string;
            if (assetId == null) {
                return;
            }
            var assetList = CollectionUtil.SingleElement(new Asset { AssetID = assetId });
            maximoTicket.Asset = assetList.ToArray();
        }


        protected override void HandleLongDescription(ServiceIncident webServiceObject, CrudOperationData entity, ApplicationMetadata metadata, bool update) {
            var problem = webServiceObject.Problem;
            problem.Description = HapagChangeHandler.ParseSchemaBasedLongDescription(entity, metadata.Schema.SchemaId);
        }

        protected override string GetAffectedPerson(CrudOperationData entity) {
            return (string)entity.GetAttribute("affectedperson");
        }

        protected string GetTemplateIdForNewChange() {
            var module = ContextManager.LookupContext().Module;
            if (!string.IsNullOrEmpty(module)) {
                if (module.Equals("sso", StringComparison.InvariantCultureIgnoreCase)) {
                    return "HLCDECHSSO";
                }
                if (module.Equals("tui", StringComparison.InvariantCultureIgnoreCase)) {
                    return "HLCDECHTUI";
                }
            }
            return "HLCDECHG";
        }

        protected override ISMServiceEntities.Problem PopulateProblem(CrudOperationData jsonObject, ServiceIncident webServiceObject,
            string entityName, bool update) {
            var problem = base.PopulateProblem(jsonObject, webServiceObject, entityName, update);
            foreach (var flexfield in problem.FlexFields.Where(f => f.mappedTo == "TemplateID")) {
                flexfield.Value = GetTemplateIdForNewChange();
                break;
            }
            return problem;
        }
    }
}
