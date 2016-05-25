using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Engine {
    public sealed class MaximoConnectorEngine : AConnectorEngine {

        //        private readonly SyncItemHandler _syncHandler;



        public MaximoConnectorEngine(EntityRepository entityRepository)
            : base(entityRepository) {
            //            _syncHandler = syncHandler;
        }

        public override TargetResult Execute(OperationWrapper operationWrapper) {
            var entityMetadata = operationWrapper.EntityMetadata;
            var connector = GenericConnectorFactory.GetConnector(entityMetadata, operationWrapper.OperationName, operationWrapper.Wsprovider);
            var operationName = operationWrapper.OperationName;

            var result = DoExecuteCrud(operationWrapper, connector);
            if (result != null) {
                return result;
            }

            //lets search for a custom operation with same name of the connector
            var mi = ReflectionUtil.GetMethodNamed(connector, operationName);
            if (mi == null) {
                //fallback to crud methods
                var isCreate = operationWrapper.Id == null;
                operationWrapper.OperationName = isCreate
                    ? OperationConstants.CRUD_CREATE
                    : OperationConstants.CRUD_UPDATE;
                return DoExecuteCrud(operationWrapper, connector);
            }

            return new MaximoCustomOperatorEngine(connector).InvokeCustomOperation(operationWrapper);
        }



        private static TargetResult DoExecuteCrud(OperationWrapper operationWrapper, IMaximoConnector connector) {
            var operationName = operationWrapper.OperationName;
            operationName = operationName.ToLower();
            if (!OperationConstants.IsCrud(operationName)) {
                //custom operation
                return null;
            }

            var crudConnector = new MaximoCrudConnectorEngine((IMaximoCrudConnector)connector);
            var crudOperationData = (CrudOperationData)operationWrapper.OperationData(null);
            operationWrapper.UserId = crudOperationData.UserId;
            switch (operationName) {
                case OperationConstants.CRUD_CREATE:
                    return crudConnector.Create(crudOperationData);
                case OperationConstants.CRUD_UPDATE:
                    return crudConnector.Update(crudOperationData);
                case OperationConstants.CRUD_DELETE:
                    return crudConnector.Delete(crudOperationData);
                case OperationConstants.CRUD_FIND_BY_ID:
                    return crudConnector.FindById(crudOperationData);
            }

            return null;
        }

        public object Create(CrudOperationData crudOperationData) {
            return Execute(new OperationWrapper(crudOperationData, OperationConstants.CRUD_CREATE));
        }

        public object Update(CrudOperationData crudOperationData) {
            return Execute(new OperationWrapper(crudOperationData, OperationConstants.CRUD_UPDATE));
        }

        public object Delete(CrudOperationData crudOperationData) {
            return Execute(new OperationWrapper(crudOperationData, OperationConstants.CRUD_DELETE));
        }

        //        public override SynchronizationApplicationData Sync(ApplicationMetadata appMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData, SyncItemHandler.SyncedItemHandlerDelegate syncItemHandlerDelegate = null) {
        //            return _syncHandler.Sync(appMetadata, applicationSyncData, syncItemHandlerDelegate);
        //        }

        sealed class MaximoCrudConnectorEngine {

            private readonly IMaximoCrudConnector _crudConnector;

            public MaximoCrudConnectorEngine(IMaximoCrudConnector crudConnector) {
                _crudConnector = crudConnector;
            }



            public TargetResult Update(CrudOperationData operationData) {
                operationData.OperationType = OperationType.AddChange;
                var proxy = _crudConnector.CreateProxy(operationData.EntityMetadata);
                var maximoTemplateData = _crudConnector.CreateExecutionContext(proxy, operationData);
                _crudConnector.PopulateIntegrationObject(maximoTemplateData);
                _crudConnector.BeforeUpdate(maximoTemplateData);
                _crudConnector.DoUpdate(maximoTemplateData);
                _crudConnector.AfterUpdate(maximoTemplateData);
                return maximoTemplateData.ResultObject;
            }

            public TargetResult Create(CrudOperationData operationData) {
                operationData.OperationType = OperationType.Add;
                var proxy = _crudConnector.CreateProxy(operationData.EntityMetadata);
                var maximoTemplateData = _crudConnector.CreateExecutionContext(proxy, operationData);
                _crudConnector.PopulateIntegrationObject(maximoTemplateData);
                _crudConnector.BeforeCreation(maximoTemplateData);
                _crudConnector.DoCreate(maximoTemplateData);
                _crudConnector.AfterCreation(maximoTemplateData);
                return maximoTemplateData.ResultObject;
            }
            public TargetResult Delete(CrudOperationData operationData) {
                operationData.OperationType = OperationType.Delete;
                var proxy = _crudConnector.CreateProxy(operationData.EntityMetadata);
                var maximoTemplateData = _crudConnector.CreateExecutionContext(proxy, operationData);
                _crudConnector.PopulateIntegrationObject(maximoTemplateData);
                _crudConnector.BeforeDeletion(maximoTemplateData);
                _crudConnector.DoDelete(maximoTemplateData);
                _crudConnector.AfterDeletion(maximoTemplateData);
                return maximoTemplateData.ResultObject;
            }

            public TargetResult FindById(CrudOperationData operationData) {
                operationData.OperationType = OperationType.Item;
                var proxy = _crudConnector.CreateProxy(operationData.EntityMetadata);
                var maximoTemplateData = _crudConnector.CreateExecutionContext(proxy, operationData);
                _crudConnector.PopulateIntegrationObject(maximoTemplateData);
                _crudConnector.BeforeFindById(maximoTemplateData);
                _crudConnector.DoFindById(maximoTemplateData);
                _crudConnector.AfterFindById(maximoTemplateData);
                return maximoTemplateData.ResultObject;
            }




        }




    }
}
