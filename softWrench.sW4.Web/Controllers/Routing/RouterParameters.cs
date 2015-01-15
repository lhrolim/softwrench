using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Web.Controllers.Routing {

    /// <summary>
    /// Class that holds all the information necessary for the framework to know where the user should be routed to after some operation has ocurred
    /// </summary>
    public class RouterParameters {
        
        public RouterParameters(ApplicationMetadata currentApplication,
            ClientPlatform platform, ApplicationMetadata nextApplication,
            string operation, bool targetMocked, TargetResult targetResult, CheckPointCrudContext checkPointContext=null) {
            CurrentApplication = currentApplication;
            Platform = platform;
            NextApplication = nextApplication;
            Operation = operation;
            TargetMocked = targetMocked;
            TargetResult = targetResult;
            CheckPointContext = checkPointContext;
        }


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

        /// <summary>
        /// 
        /// </summary>
        public CheckPointCrudContext CheckPointContext { get; set; }


    }
}