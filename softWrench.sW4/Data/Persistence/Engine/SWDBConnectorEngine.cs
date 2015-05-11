using System;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.Sync;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Engine {
    class SWDBConnectorEngine : AConnectorEngine {


        public SWDBConnectorEngine(EntityRepository entityRepository) : base(entityRepository) { }

        public override SynchronizationApplicationData Sync(ApplicationMetadata applicationMetadata,
            SynchronizationRequestDto.ApplicationSyncData applicationSyncData,
            SyncItemHandler.SyncedItemHandlerDelegate syncItemHandlerDelegate = null) {
            throw new NotImplementedException();
        }

        public override TargetResult Execute(OperationWrapper operationWrapper) {
            throw new NotImplementedException();
            //var entityMetadata = operationWrapper.EntityMetadata;
            //var connector = GenericConnectorFactory.GetConnector(entityMetadata, operationWrapper.OperationName);
            //var operationName = operationWrapper.OperationName;
            //var result = DoExecuteCrud(operationWrapper, connector);
            //if (result != null) {
            //    return result;
            //}

            ////lets search for a custom operation with same name of the connector
            //var mi = ReflectionUtil.GetMethodNamed(connector, operationName);
            //if (mi == null) {
            //    //fallback to crud methods
            //    var isCreate = operationWrapper.Id == null;
            //    operationWrapper.OperationName = isCreate
            //        ? OperationConstants.CRUD_CREATE
            //        : OperationConstants.CRUD_UPDATE;
            //    return DoExecuteCrud(operationWrapper, connector);
            //}

            //return new MaximoCustomOperatorEngine(connector).InvokeCustomOperation(operationWrapper);
        }


    }
}
