using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;

namespace softWrench.sW4.Data.Persistence.Engine {
    public abstract class AConnectorEngine : IConnectorEngine {
        //        public abstract SynchronizationApplicationData Sync(ApplicationMetadata applicationMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData,
        //            SyncItemHandler.SyncedItemHandlerDelegate syncItemHandlerDelegate = null);

        public abstract TargetResult Execute(OperationWrapper operationWrapper);


        protected readonly EntityRepository EntityRepository;


        protected AConnectorEngine(EntityRepository entityRepository) {
            EntityRepository = entityRepository;
        }

        public virtual async Task<int> Count(EntityMetadata entityMetadata, SearchRequestDto searchDto) {
            return await EntityRepository.Count(entityMetadata, searchDto);
        }

        public virtual async Task<AttributeHolder> FindById(SlicedEntityMetadata entityMetadata, string id, Tuple<string, string> userIdSitetuple) {
            if (id == null && userIdSitetuple == null) {
                throw new InvalidOperationException("either id or userid needs to be provided");
            }
            if (id != null) {
                return await EntityRepository.Get(entityMetadata, id);
            }
            return await EntityRepository.ByUserIdSite(entityMetadata, userIdSitetuple);
        }

        public virtual async Task<IReadOnlyList<AttributeHolder>> Find(SlicedEntityMetadata slicedEntityMetadata, PaginatedSearchRequestDto searchDto) {
            return await Find(slicedEntityMetadata, searchDto, null);
        }



        public virtual async Task<IReadOnlyList<AttributeHolder>> Find(SlicedEntityMetadata slicedEntityMetadata, PaginatedSearchRequestDto searchDto,
            IDictionary<string, ApplicationCompositionSchema> compositionSchemas) {
            var list = await EntityRepository.Get(slicedEntityMetadata, searchDto);



            return list;
        }


    }
}
