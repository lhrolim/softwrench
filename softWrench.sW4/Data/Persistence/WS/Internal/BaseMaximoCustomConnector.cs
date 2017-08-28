using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using cts.commons.simpleinjector;
using softwrench.sw4.api.classes.integration;
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Data.Persistence.WS.Internal {
    public abstract class BaseMaximoCustomConnector : IConnectorDecorator {
        public MaximoConnectorEngine Maximoengine {
            get {
                return
                    SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(
                        typeof(MaximoConnectorEngine));
            }
        }

        protected MaximoOperationExecutionContext GetContext(IOperationData operationData) {
            return MaximoOperationExecutionContext.GetInstance(operationData, null);
        }

        protected MaximoOperationExecutionContext GetContext(ApplicationMetadata applicationMetadata, EntityMetadata entityMetadata, OperationType operationType, string id = null) {
            return MaximoOperationExecutionContext.GetInstance(new BaseOperationData(entityMetadata, applicationMetadata, operationType, id), null);
        }


        class BaseOperationData : IOperationData {
            public string Id {
                get; set;
            }
            public string UserId {
                get; set;
            }
            public string Class {
                get {
                    return EntityMetadata.GetTableName();
                }
            }
            public EntityMetadata EntityMetadata {
                get; set;
            }
            public OperationType OperationType {
                get; set;
            }
            public OperationProblemData ProblemData {
                get; set;
            }
            public AttributeHolder Holder {
                get {
                    return null;
                }
            }

            public ApplicationMetadata ApplicationMetadata {
                get; set;
            }

            public BaseOperationData(EntityMetadata entityMetadata, ApplicationMetadata applicationMetadata, OperationType operationType, string id = null, string userId = null) {
                Id = id;
                UserId = userId;
                EntityMetadata = entityMetadata;
                OperationType = operationType;
                ApplicationMetadata = ApplicationMetadata;
            }
        }

        public abstract string ApplicationName();
        public virtual string ClientFilter() {
            return null;
        }
        public abstract string ActionId();
    }
}