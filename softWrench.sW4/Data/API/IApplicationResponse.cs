using softWrench.sW4.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Data.API {
    public interface IApplicationResponse :IGenericResponseResult {
        string Type { get; }
        ApplicationSchemaDefinition Schema { get; set; }
        string CachedSchemaId {get; set;}

        string Mode { get; set; }

        string ApplicationName { get; }

       
    }
}