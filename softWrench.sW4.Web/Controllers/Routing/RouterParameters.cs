using System.Collections.Generic;
using cts.commons.Util;
using JetBrains.Annotations;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Web.Controllers.Routing {

    /// <summary>
    /// Class that holds all the information necessary for the framework to know where the user should be routed to after some operation has ocurred
    /// </summary>
    public class RouterParameters {


        private ApplicationMetadata _nextApplication;
        private string _nextController;
        private string _nextAction;

        /// <summary>
        /// The next application we should redirect the user to
        /// </summary>
        [NotNull]
        public ApplicationMetadata NextApplication {
            get {
                if (TargetResult != null && TargetResult.NextApplication != null) {
                    //let´s give a chance that maximo connectors change the next application the user needs to get redirected based upon complex flows
                    return TargetResult.NextApplication;
                }
                return _nextApplication;
            }
            set {
                _nextApplication = value;
            }
        }


        public string NextController {
            get {
                if (TargetResult != null && TargetResult.NextController != null) {
                    //let´s give a chance that maximo connectors change the next application the user needs to get redirected based upon complex flows
                    return TargetResult.NextController;
                }

                return _nextController;
            }
        }

        public string NextAction {
            get {
                if (TargetResult != null && TargetResult.NextAction != null) {
                    //let´s give a chance that maximo connectors change the next application the user needs to get redirected based upon complex flows
                    return TargetResult.NextAction;
                }
                return _nextAction;
            }
        }

        public RouterParameters(ApplicationMetadata currentApplication,
            ClientPlatform platform, RouterParametersDTO routerDTO,
            string operation, bool targetMocked, TargetResult targetResult, InMemoryUser user, ApplicationMetadataSchemaKey resolvedNextSchemaKey = null) {
            CurrentApplication = currentApplication;
            Platform = platform;
            Operation = operation;
            TargetMocked = targetMocked;
            TargetResult = targetResult;
            CheckPointContext = new Dictionary<ApplicationKey, CheckPointCrudContext>();
            if (routerDTO.CheckPointData != null) {
                CheckPointContext = new Dictionary<ApplicationKey, CheckPointCrudContext>();
                foreach (var checkpointData in routerDTO.CheckPointData) {
                    var applicationKey = checkpointData.GetApplicationKey();
                    if (!CheckPointContext.ContainsKey(applicationKey)) {
                        CheckPointContext.Add(applicationKey, checkpointData);
                    } else {
                        //playing safe SWWEB-871 --> better to redirect to wrong filter then interrupting
                        LoggingUtil.DefaultLog.WarnFormat("duplicate checkpoint entry found for {0}", applicationKey);
                    }
                }
            }
            User = user;
            //we could have a custom next action/controller to be executed, although usually it would stay in the crud application context
            FillNextActionAndController(currentApplication, routerDTO);
            NextApplication = RouteParameterManager.FillNextSchema(currentApplication, routerDTO, platform, user,Operation, resolvedNextSchemaKey);
            DispatcherComposition = routerDTO.DispatcherComposition;
            }

        


        private void FillNextActionAndController(ApplicationMetadata currentApplication, RouterParametersDTO routerDTO) {
            if (routerDTO.NextController != null && routerDTO.NextAction != null) {
                _nextController = routerDTO.NextController;
                _nextAction = routerDTO.NextAction;
            } else if (currentApplication.Schema.Properties.ContainsKey(ApplicationSchemaPropertiesCatalog.AfterSubmitAction)) {
                var afterSubmitAction =
                    currentApplication.Schema.Properties[ApplicationSchemaPropertiesCatalog.AfterSubmitAction];
                var controllerAndAction = afterSubmitAction.Split('.');
                _nextController = controllerAndAction[0];
                _nextAction = controllerAndAction[1];
            }
        }

        public string DispatcherComposition {
            get; set;
        }

        public InMemoryUser User {
            get; set;
        }

        /// <summary>
        /// The current Application that the system was prior to performing the routing operation
        /// </summary>
        public ApplicationMetadata CurrentApplication {
            get; set;
        }

        /// <summary>
        /// The current schema that the system was prior to performing the routing
        /// </summary>
        public ApplicationMetadataSchemaKey CurrentKey {
            get {
                return CurrentApplication.Schema.GetSchemaKey();
            }
        }

        public ClientPlatform Platform {
            get; set;
        }






        /// <summary>
        /// The next schema we should redirect the user to
        /// </summary>
        public ApplicationMetadataSchemaKey NextKey {
            get {
                return NextApplication.Schema.GetSchemaKey();
            }
        }


        public bool NoApplicationRedirectDetected {
            get {
                return NextApplication.Equals(CurrentApplication);
            }
        }

        /// <summary>
        /// The current operation that has just been performed on the backend (i.e SAVE,UPDATE, DELETE, or some custom one). There might be some custom hooks depending on the type of the operation that has just occurred.
        /// </summary>
        public string Operation {
            get; set;
        }

        /// <summary>
        /// whether or not the target backend (maximo, SAP, etc...) was marked to be mocked. 
        /// If true, we should generate fake values for the next schema screen, since the operation hasn´t actually been called.
        /// </summary>
        public bool TargetMocked {
            get; set;
        }

        /// <summary>
        /// the result of the target backeend system invocation
        /// </summary>
        public TargetResult TargetResult {
            get; set;
        }




        /// <summary>
        /// Specifies exactly how a certain schema should be routed to. 
        /// </summary>
        [NotNull]
        public IDictionary<ApplicationKey, CheckPointCrudContext> CheckPointContext {
            get; set;
        }


    }
}