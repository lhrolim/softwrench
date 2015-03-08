using System;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Persistence.WS.Internal {
    public abstract class BaseMaximoCustomConnector : IMaximoConnector {
        internal MaximoConnectorEngine Maximoengine {
            get {
                return
                    SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(
                        typeof(MaximoConnectorEngine));
            }
        }

        protected MaximoOperationExecutionContext GetContext(OperationData operationData) {
            return MaximoOperationExecutionContext.GetInstance(operationData, null);
        }

        protected MaximoOperationExecutionContext GetContext(ApplicationMetadata applicationMetadata, EntityMetadata entityMetadata, OperationType operationType, string id = null) {
            return MaximoOperationExecutionContext.GetInstance(new BaseOperationData(entityMetadata, applicationMetadata, operationType, id), null);
        }


        class BaseOperationData : IOperationData {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string Class { get { return EntityMetadata.GetTableName(); } }
            public EntityMetadata EntityMetadata { get; set; }
            public OperationType OperationType { get; set; }

            public ApplicationMetadata ApplicationMetadata { get; set; }

            public BaseOperationData(EntityMetadata entityMetadata, ApplicationMetadata applicationMetadata, OperationType operationType, string id = null, string userId = null) {
                Id = id;
                UserId = userId;
                EntityMetadata = entityMetadata;
                OperationType = operationType;
                ApplicationMetadata = ApplicationMetadata;
            }
        }
    }
}