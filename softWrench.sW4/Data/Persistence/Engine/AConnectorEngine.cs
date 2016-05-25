using System;
using System.Collections.Generic;
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

        public int Count(EntityMetadata entityMetadata, SearchRequestDto searchDto) {
            return _entityRepository.Count(entityMetadata, searchDto);
        }

        public AttributeHolder FindById(SlicedEntityMetadata entityMetadata, string id, Tuple<string, string> userIdSitetuple) {
            if (id == null && userIdSitetuple == null) {
                throw new InvalidOperationException("either id or userid needs to be provided");
            }
            if (id != null) {
                return _entityRepository.Get(entityMetadata, id);
            }
            return _entityRepository.ByUserIdSite(entityMetadata, userIdSitetuple);
        }

        public IReadOnlyList<AttributeHolder> Find(SlicedEntityMetadata slicedEntityMetadata, PaginatedSearchRequestDto searchDto) {
            return Find(slicedEntityMetadata, searchDto, null);
        }



        public IReadOnlyList<AttributeHolder> Find(SlicedEntityMetadata slicedEntityMetadata, PaginatedSearchRequestDto searchDto,
            IDictionary<string, ApplicationCompositionSchema> compositionSchemas) {
            var list = _entityRepository.Get(slicedEntityMetadata, searchDto);



            return list;
        }


    }
}
