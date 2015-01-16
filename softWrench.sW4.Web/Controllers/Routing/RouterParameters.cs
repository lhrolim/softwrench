using System;
using System.Linq;
using System.Web.Providers.Entities;
using JetBrains.Annotations;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Command;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Util;

namespace softWrench.sW4.Web.Controllers.Routing {

    /// <summary>
    /// Class that holds all the information necessary for the framework to know where the user should be routed to after some operation has ocurred
    /// </summary>
    public class RouterParameters {


        public RouterParameters(ApplicationMetadata currentApplication,
            ClientPlatform platform, RouterParametersDTO routerDTO,
            string operation, bool targetMocked, TargetResult targetResult, InMemoryUser user, ApplicationMetadataSchemaKey resolvedNextSchemaKey = null) {
            CurrentApplication = currentApplication;
            Platform = platform;
            Operation = operation;
            TargetMocked = targetMocked;
            TargetResult = targetResult;
            CheckPointContext = routerDTO.CheckPointData ?? new CheckPointCrudContext();
            User = user;

            //we could have a custom next action/controller to be executed, although usually it would stay in the crud application context
            FillNextActionAndController(currentApplication, routerDTO);
            FillNextSchema(currentApplication, routerDTO, platform, user, resolvedNextSchemaKey);
        }

        private void FillNextSchema(ApplicationMetadata currentApplication, RouterParametersDTO routerDTO, ClientPlatform platform, InMemoryUser user, ApplicationMetadataSchemaKey resolvedNextSchemaKey) {

            var nextSchema = resolvedNextSchemaKey;
            if (nextSchema == null) {
                //legacy support for operations... they are still coming inside the json
                //TODO: remove
                var nextSchemaKey = routerDTO.NextSchemaKey;
                if (nextSchemaKey == null) {

                    var applicationCommand = ApplicationCommandUtils.GetApplicationCommand(currentApplication, Operation);
                    if (applicationCommand != null && !String.IsNullOrWhiteSpace(applicationCommand.NextSchemaId)) {
                        nextSchemaKey = applicationCommand.NextSchemaId;
                    } else {
                        //if not specified from the client, let´s search on the schema definition
                        nextSchemaKey =
                            currentApplication.Schema.GetProperty(ApplicationSchemaPropertiesCatalog.RoutingNextSchemaId);
                    }
                }
                if (nextSchemaKey != null) {
                    nextSchema = SchemaUtil.GetSchemaKeyFromString(nextSchemaKey, platform);
                } else {
                    //if it was still not specified in any place, stay on the same schema
                    NextApplication = currentApplication;
                    return;
                }
            }


            var nextApplicationName = routerDTO.NextApplicationName;
            if (nextApplicationName == null) {
                nextApplicationName =
                    currentApplication.Schema.GetProperty(ApplicationSchemaPropertiesCatalog.RoutingNextApplication);
                if (nextApplicationName == null) {
                    //use the same application as current by default,since it´s rare to 
                    nextApplicationName = currentApplication.Name;
                }
            }
            var nextApplication = MetadataProvider.Application(nextApplicationName);
            NextApplication = nextApplication.ApplyPolicies(nextSchema, user, platform);
        }

        private void FillNextActionAndController(ApplicationMetadata currentApplication, RouterParametersDTO routerDTO) {
            if (routerDTO.NextController != null && routerDTO.NextAction != null) {
                NextController = routerDTO.NextController;
                NextAction = routerDTO.NextAction;
            } else if (currentApplication.Schema.Properties.ContainsKey(ApplicationSchemaPropertiesCatalog.AfterSubmitAction)) {
                var afterSubmitAction =
                    currentApplication.Schema.Properties[ApplicationSchemaPropertiesCatalog.AfterSubmitAction];
                var controllerAndAction = afterSubmitAction.Split('.');
                NextController = controllerAndAction[0];
                NextAction = controllerAndAction[1];
            }
        }


        public InMemoryUser User { get; set; }

        /// <summary>
        /// The current Application that the system was prior to performing the routing operation
        /// </summary>
        public ApplicationMetadata CurrentApplication { get; set; }

        /// <summary>
        /// The current schema that the system was prior to performing the routing
        /// </summary>
        public ApplicationMetadataSchemaKey CurrentKey { get { return CurrentApplication.Schema.GetSchemaKey(); } }

        public ClientPlatform Platform { get; set; }

        /// <summary>
        /// The next application we should redirect the user to
        /// </summary>
        public ApplicationMetadata NextApplication { get; set; }


        /// <summary>
        /// The next schema we should redirect the user to
        /// </summary>
        public ApplicationMetadataSchemaKey NextKey { get { return NextApplication.Schema.GetSchemaKey(); } }


        public bool NoApplicationRedirectDetected { get { return NextApplication == CurrentApplication; } }

        /// <summary>
        /// The current operation that has just been performed on the backend (i.e SAVE,UPDATE, DELETE, or some custom one). There might be some custom hooks depending on the type of the operation that has just occurred.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// whether or not the target backend (maximo, SAP, etc...) was marked to be mocked. 
        /// If true, we should generate fake values for the next schema screen, since the operation hasn´t actually been called.
        /// </summary>
        public bool TargetMocked { get; set; }

        /// <summary>
        /// the result of the target backeend system invocation
        /// </summary>
        public TargetResult TargetResult { get; set; }

        public string NextController { get; private set; }

        public string NextAction { get; private set; }


        /// <summary>
        /// Specifies exactly how a certain schema should be routed to. 
        /// </summary>
        [NotNull]
        public CheckPointCrudContext CheckPointContext { get; set; }


    }
}