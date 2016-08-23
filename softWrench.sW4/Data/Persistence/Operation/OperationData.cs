using JetBrains.Annotations;
using softwrench.sw4.api.classes.integration;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;

namespace softWrench.sW4.Data.Persistence.Operation {
    /// <summary>
    /// Class that contains data of a given server operation.
    /// </summary>
    public abstract class OperationData : IOperationData {
        public string UserId { get; set; }
        public string Class { get { return EntityMetadata.GetTableName(); } }
        public EntityMetadata EntityMetadata { get; set; }
        public ApplicationMetadata ApplicationMetadata { get; set; }
        private OperationType _operationType = OperationType.AddChange;
        public string Id { get; set; }

        [CanBeNull]
        public OperationProblemData ProblemData { get; set; }

        public virtual AttributeHolder Holder { get { return null; } }


        protected OperationData() {

        }

        protected OperationData(EntityMetadata entityMetadata) {
            EntityMetadata = entityMetadata;
        }

        public OperationType OperationType {
            get { return _operationType; }
            set { _operationType = value; }
        }



    }
}
