using softwrench.sw4.api.classes.integration;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;

namespace softWrench.sW4.Data.Persistence.Operation {
    public interface IOperationData :ICommonOperationData{
        string Id { get; set; }

        string UserId { get; set; }

        string Class { get; }

        EntityMetadata EntityMetadata { get; set; }

        ApplicationMetadata ApplicationMetadata { get; set; }

        OperationType OperationType { get; set; }

        OperationProblemData ProblemData {
            get; set;
        }
    }
}