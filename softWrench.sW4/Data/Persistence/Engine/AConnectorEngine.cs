using System.Collections.Generic;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.Collection;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.Persistence.Engine {
    public abstract class AConnectorEngine : IConnectorEngine {
//        public abstract SynchronizationApplicationData Sync(ApplicationMetadata applicationMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData,
//            SyncItemHandler.SyncedItemHandlerDelegate syncItemHandlerDelegate = null);

        public abstract TargetResult Execute(OperationWrapper operationWrapper);

        private readonly CollectionResolver _collectionResolver = new CollectionResolver();

        private readonly EntityRepository _entityRepository;

        protected AConnectorEngine(EntityRepository entityRepository) {
            _entityRepository = entityRepository;
        }

        public int Count(EntityMetadata entityMetadata, SearchRequestDto searchDto) {
            return _entityRepository.Count(entityMetadata, searchDto);
        }

        public AttributeHolder FindById(ApplicationSchemaDefinition schema, SlicedEntityMetadata entityMetadata, string id,
            IDictionary<string, ApplicationCompositionSchema> compositionSchemas) {
                var mainEntity = _entityRepository.Get(entityMetadata, id);
            if (mainEntity == null) {
                return null;
            }
            //            if ("true".EqualsIc(schema.GetProperty(ApplicationSchemaPropertiesCatalog.PreFetchCompositions))){
            _collectionResolver.ResolveCollections(entityMetadata, compositionSchemas, mainEntity);
            //            }

            return mainEntity;
        }

        public IReadOnlyList<AttributeHolder> Find(SlicedEntityMetadata slicedEntityMetadata, PaginatedSearchRequestDto searchDto) {
            return Find(slicedEntityMetadata, searchDto, null);
        }



        public IReadOnlyList<AttributeHolder> Find(SlicedEntityMetadata slicedEntityMetadata, PaginatedSearchRequestDto searchDto,
            IDictionary<string, ApplicationCompositionSchema> compositionSchemas) {
                var list = _entityRepository.Get(slicedEntityMetadata, searchDto);

            // Get the composition data for the list, only in the case of detailed list (like printing details), otherwise, this is unecessary
            if (compositionSchemas != null && compositionSchemas.Count > 0) {
                _collectionResolver.ResolveCollections(slicedEntityMetadata, compositionSchemas, list);
            }

            return list;
        }

    }
}
