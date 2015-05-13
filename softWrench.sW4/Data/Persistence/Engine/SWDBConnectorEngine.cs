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
        }


    }
}
