namespace softWrench.sW4.Data.Persistence.Operation
{
    public abstract class CrudOperationDataContainer : OperationData
    {
        internal CrudOperationData _crudData;

        public CrudOperationData CrudData
        {
            get { return _crudData; }
            set { _crudData = value; }
        }
    }
}
