namespace softWrench.sW4.Data.Persistence.Operation {
    public abstract class CrudOperationDataContainer : OperationData {
        internal CrudOperationData _crudData;


        protected CrudOperationDataContainer() {

        }

        protected CrudOperationDataContainer(CrudOperationData crudData) {
            CrudData = crudData;
        }


        public CrudOperationData CrudData {
            get {
                return _crudData;
            }
            set {
                _crudData = value;
                if (value != null) {
                    EntityMetadata = value.EntityMetadata;
                    ApplicationMetadata = value.ApplicationMetadata;
                    Id = value.Id;
                    UserId = value.UserId;
                }

            }
        }
    }
}
