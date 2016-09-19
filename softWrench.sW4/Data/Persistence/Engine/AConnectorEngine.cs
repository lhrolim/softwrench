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


        private readonly EntityRepository _entityRepository;


        protected AConnectorEngine(EntityRepository entityRepository) {
            _entityRepository = entityRepository;
        }

        public async Task<int> Count(EntityMetadata entityMetadata, SearchRequestDto searchDto) {
            return await _entityRepository.Count(entityMetadata, searchDto);
        }

        public async Task<AttributeHolder> FindById(SlicedEntityMetadata entityMetadata, string id, Tuple<string, string> userIdSitetuple) {
            if (id == null && userIdSitetuple == null) {
                throw new InvalidOperationException("either id or userid needs to be provided");
            }
            if (id != null) {
                return await _entityRepository.Get(entityMetadata, id);
            }
            return await _entityRepository.ByUserIdSite(entityMetadata, userIdSitetuple);
        }

        public async Task<IReadOnlyList<AttributeHolder>> Find(SlicedEntityMetadata slicedEntityMetadata, PaginatedSearchRequestDto searchDto) {
            return await Find(slicedEntityMetadata, searchDto, null);
        }



        public async Task<IReadOnlyList<AttributeHolder>> Find(SlicedEntityMetadata slicedEntityMetadata, PaginatedSearchRequestDto searchDto,
            IDictionary<string, ApplicationCompositionSchema> compositionSchemas) {
            var list = await _entityRepository.Get(slicedEntityMetadata, searchDto);



            return list;
        }


    }
}
