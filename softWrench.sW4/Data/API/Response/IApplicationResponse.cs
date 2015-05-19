using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Data.API.Response {
    public interface IApplicationResponse :IGenericResponseResult {
        string Type { get; }
        ApplicationSchemaDefinition Schema { get; }

        string Mode { get; set; }

        string ApplicationName { get; }

        string Id { get; }

       
    }
}