using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Persistence.Engine {
    public interface IConnectorEngine : ISingletonComponent {
        Task<int> Count(EntityMetadata entityMetadata, SearchRequestDto searchDto);
        Task<AttributeHolder> FindById(SlicedEntityMetadata entityMetadata, string id, Tuple<string, string> userIdSitetuple);
        Task<IReadOnlyList<AttributeHolder>> Find(SlicedEntityMetadata entityMetadata, PaginatedSearchRequestDto searchDto, IDictionary<string, ApplicationCompositionSchema> applicationCompositionSchemata);
//        SynchronizationApplicationData Sync(ApplicationMetadata applicationMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData, SyncItemHandler.SyncedItemHandlerDelegate syncItemHandlerDelegate = null);
        TargetResult Execute(OperationWrapper operationWrapper);
    }
}