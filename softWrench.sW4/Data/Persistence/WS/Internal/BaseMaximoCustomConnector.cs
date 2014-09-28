using System;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;

namespace softWrench.sW4.Data.Persistence.WS.Internal {
    public abstract class BaseMaximoCustomConnector : IMaximoConnector, IDisposable {
        internal MaximoConnectorEngine Maximoengine = new MaximoConnectorEngine();

        protected MaximoOperationExecutionContext GetContext(OperationData operationData) {
            return MaximoOperationExecutionContext.GetInstance(operationData, null);
        }

        protected MaximoOperationExecutionContext GetContext(ApplicationMetadata applicationMetadata, EntityMetadata entityMetadata, OperationType operationType, string id = null) {
            return MaximoOperationExecutionContext.GetInstance(new BaseOperationData(entityMetadata, applicationMetadata, operationType, id), null);
        }

        public void Dispose() {
            Maximoengine.Dispose();
        }

        class BaseOperationData : IOperationData {
            public string Id { get; set; }
            public string Class { get { return EntityMetadata.GetTableName(); } }
            public EntityMetadata EntityMetadata { get; set; }
            public OperationType OperationType { get; set; }

            public ApplicationMetadata ApplicationMetadata { get; set; }

            public BaseOperationData(EntityMetadata entityMetadata, ApplicationMetadata applicationMetadata, OperationType operationType, string id = null) {
                Id = id;
                EntityMetadata = entityMetadata;
                OperationType = operationType;
                ApplicationMetadata = ApplicationMetadata;
            }
        }
    }
}