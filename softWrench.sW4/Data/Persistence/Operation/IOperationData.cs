using softwrench.sw4.api.classes.integration;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;

namespace softWrench.sW4.Data.Persistence.Operation {
    public interface IOperationData : ICommonOperationData {

        EntityMetadata EntityMetadata {
            get; set;
        }


        OperationType OperationType {
            get; set;
        }

    }
}