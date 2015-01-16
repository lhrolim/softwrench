using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.API {
    
    /// <summary>
    /// This class groups the parameters that can be actually modified on the client invocation, since most of them are not customizable
    /// </summary>
    public class RouterParametersDTO {

        public string NextApplicationName { get; set; }
        
        public string NextSchemaKey { get; set; }

        public CheckPointCrudContext CheckPointData { get; set; }

        public string NextController { get; set; }

        public string NextAction { get; set; }


    }
}