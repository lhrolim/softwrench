using System.Linq;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.Sync;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data.Persistence.WS.API {
    public sealed class MaximoConnectorEngine : ISingletonComponent {

        private readonly EntityRepository _entityRepository;

        private readonly SyncItemHandler _syncHandler = new SyncItemHandler();

        private readonly CollectionResolver _collectionResolver;

        public MaximoConnectorEngine(EntityRepository entityRepository, CollectionResolver collectionResolver) {
            _entityRepository = entityRepository;
            _collectionResolver = collectionResolver;
        }


        public MaximoResult Execute(OperationWrapper operationWrapper) {
            var entityMetadata = operationWrapper.EntityMetadata;
            var connector = GenericConnectorFactory.GetConnector(entityMetadata, operationWrapper.OperationName);
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

        private static MaximoResult DoExecuteCrud(OperationWrapper operationWrapper, IMaximoConnector connector) {

            if (!OperationConstants.IsCrud(operationWrapper.OperationName)) {
                //custom operation
                return null;
            }

            var crudConnector = new MaximoCrudConnectorEngine((IMaximoCrudConnector)connector);
            var crudOperationData = (CrudOperationData)operationWrapper.OperationData(null);
            switch (operationWrapper.OperationName) {
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

        public int Count(EntityMetadata entityMetadata, SearchRequestDto searchDto) {
            return _entityRepository.Count(entityMetadata, searchDto);
        }

        public AttributeHolder FindById(ApplicationSchemaDefinition schema, SlicedEntityMetadata entityMetadata, string id,
            IDictionary<string, ApplicationCompositionSchema> compositionSchemas) {
            var mainEntity = _entityRepository.Get(entityMetadata, id);
            if (mainEntity == null) {
                return null;
            }
            if ("true".EqualsIc(schema.GetProperty(ApplicationSchemaPropertiesCatalog.PreFetchCompositions))) {
                _collectionResolver.ResolveCollections(entityMetadata, compositionSchemas, mainEntity);
            }
            var compostionsToUse = new Dictionary<string, ApplicationCompositionSchema>();

            foreach (var compositionEntry in compositionSchemas) {
                if (FetchType.Eager.Equals(compositionEntry.Value.FetchType) || compositionEntry.Value.INLINE) {
                    compostionsToUse.Add(compositionEntry.Key, compositionEntry.Value);
                }
            }
            if (compostionsToUse.Any()) {
                _collectionResolver.ResolveCollections(entityMetadata, compostionsToUse, mainEntity);
            }

            return mainEntity;
        }

        public IReadOnlyList<AttributeHolder> Find(SlicedEntityMetadata slicedEntityMetadata, PaginatedSearchRequestDto searchDto) {
            return Find(slicedEntityMetadata, searchDto, null);
        }

        public IReadOnlyList<AttributeHolder> Find(SlicedEntityMetadata slicedEntityMetadata, PaginatedSearchRequestDto searchDto,
            IDictionary<string, ApplicationCompositionSchema> compositionSchemas) {
            var list = _entityRepository.Get(slicedEntityMetadata, searchDto);
            return list;
        }

        public SynchronizationApplicationData Sync(ApplicationMetadata appMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData, SyncItemHandler.SyncedItemHandlerDelegate syncItemHandlerDelegate = null) {
            return _syncHandler.Sync(appMetadata, applicationSyncData, syncItemHandlerDelegate);
        }

        sealed class MaximoCrudConnectorEngine : IDisposable {

            private readonly IMaximoCrudConnector _crudConnector;

            public MaximoCrudConnectorEngine(IMaximoCrudConnector crudConnector) {
                _crudConnector = crudConnector;
            }


            public MaximoResult Update(CrudOperationData operationData) {
                operationData.OperationType = OperationType.AddChange;
                var proxy = _crudConnector.CreateProxy(operationData.EntityMetadata);
                var maximoTemplateData = _crudConnector.CreateExecutionContext(proxy, operationData);
                _crudConnector.PopulateIntegrationObject(maximoTemplateData);
                _crudConnector.BeforeUpdate(maximoTemplateData);
                _crudConnector.DoUpdate(maximoTemplateData);
                _crudConnector.AfterUpdate(maximoTemplateData);
                return maximoTemplateData.ResultObject;
            }

            public MaximoResult Create(CrudOperationData operationData) {
                operationData.OperationType = OperationType.Add;
                var proxy = _crudConnector.CreateProxy(operationData.EntityMetadata);
                var maximoTemplateData = _crudConnector.CreateExecutionContext(proxy, operationData);
                _crudConnector.PopulateIntegrationObject(maximoTemplateData);
                _crudConnector.BeforeCreation(maximoTemplateData);
                _crudConnector.DoCreate(maximoTemplateData);
                _crudConnector.AfterCreation(maximoTemplateData);
                return maximoTemplateData.ResultObject;
            }
            public MaximoResult Delete(CrudOperationData operationData) {
                operationData.OperationType = OperationType.Delete;
                var proxy = _crudConnector.CreateProxy(operationData.EntityMetadata);
                var maximoTemplateData = _crudConnector.CreateExecutionContext(proxy, operationData);
                _crudConnector.PopulateIntegrationObject(maximoTemplateData);
                _crudConnector.BeforeDeletion(maximoTemplateData);
                _crudConnector.DoDelete(maximoTemplateData);
                _crudConnector.AfterDeletion(maximoTemplateData);
                return maximoTemplateData.ResultObject;
            }

            public MaximoResult FindById(CrudOperationData operationData) {
                operationData.OperationType = OperationType.Item;
                var proxy = _crudConnector.CreateProxy(operationData.EntityMetadata);
                var maximoTemplateData = _crudConnector.CreateExecutionContext(proxy, operationData);
                _crudConnector.PopulateIntegrationObject(maximoTemplateData);
                _crudConnector.BeforeFindById(maximoTemplateData);
                _crudConnector.DoFindById(maximoTemplateData);
                _crudConnector.AfterFindById(maximoTemplateData);
                return maximoTemplateData.ResultObject;
            }


            public void Dispose() {
            }
        }



    }
}
