using System.Collections.Generic;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Sync;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Persistence.Engine {
    public interface IConnectorEngine : IComponent {
        int Count(EntityMetadata entityMetadata, SearchRequestDto searchDto);
        AttributeHolder FindById(ApplicationSchemaDefinition schema, SlicedEntityMetadata entityMetadata, string id, IDictionary<string, ApplicationCompositionSchema> applicationCompositionSchemas);
        IReadOnlyList<AttributeHolder> Find(SlicedEntityMetadata entityMetadata, PaginatedSearchRequestDto searchDto, IDictionary<string, ApplicationCompositionSchema> applicationCompositionSchemata);
        SynchronizationApplicationData Sync(ApplicationMetadata applicationMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData, SyncItemHandler.SyncedItemHandlerDelegate syncItemHandlerDelegate = null);
        TargetResult Execute(OperationWrapper operationWrapper);
    }
}