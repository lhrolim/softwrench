using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.Util;
using log4net;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Security.Context;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.Collection {
    public class CollectionResolver : ISingletonComponent {

        private readonly EntityRepository.EntityRepository _repository;
        private readonly IContextLookuper _contextLookuper;

        public CollectionResolver(EntityRepository.EntityRepository repository, IContextLookuper contextLookuper) {
            _repository = repository;
            _contextLookuper = contextLookuper;
        }


        private EntityRepository.EntityRepository EntityRepository {
            get {
                return _repository;
            }
        }

        protected readonly ILog Log = LogManager.GetLogger(typeof(CollectionResolver));



        protected IContextLookuper ContextLookuper {
            get {
                return _contextLookuper;
            }
        }


        public async Task<IDictionary<string, EntityRepository.EntityRepository.SearchEntityResult>> ResolveCollections(SlicedEntityMetadata entityMetadata, IDictionary<string, ApplicationCompositionSchema>
            compositionSchemas, IEnumerable<AttributeHolder> attributeHolders) {
            return await DoResolveCollections(new CollectionResolverParameters(compositionSchemas, entityMetadata, attributeHolders));
        }

        public async Task<IDictionary<string, EntityRepository.EntityRepository.SearchEntityResult>> ResolveCollections(SlicedEntityMetadata entityMetadata, IDictionary<string, ApplicationCompositionSchema>
            compositionSchemas, AttributeHolder attributeHolders, PaginatedSearchRequestDto paginatedSearch = null) {
            return await DoResolveCollections(new CollectionResolverParameters(compositionSchemas, entityMetadata, new List<AttributeHolder> { attributeHolders }), paginatedSearch);
        }


        public async Task<IDictionary<string, EntityRepository.EntityRepository.SearchEntityResult>> ResolveCollections(CollectionResolverParameters parameters) {
            return await DoResolveCollections(parameters);
        }

        private async Task<IDictionary<string, EntityRepository.EntityRepository.SearchEntityResult>> DoResolveCollections(CollectionResolverParameters parameters, PaginatedSearchRequestDto paginatedSearch = null) {
            var compositionSchemas = parameters.CompositionSchemas;
            var entityMetadata = parameters.SlicedEntity;

            if (!compositionSchemas.Any()) {
                return new Dictionary<string, EntityRepository.EntityRepository.SearchEntityResult>();
            }

            var before = Stopwatch.StartNew();
            Log.DebugFormat("Init Collection Resolving for {0} Collections", String.Join(",", compositionSchemas.Keys));

            var collectionAssociations = entityMetadata
                .ListAssociations()
                .Where(entityAssociation =>
                    compositionSchemas.Keys.Contains(entityAssociation.Qualifier))
                .ToList();

            var results = new ConcurrentDictionary<string, EntityRepository.EntityRepository.SearchEntityResult>();
            // only a single composition being fetched: do it in the same Thread
            if (collectionAssociations.Count == 1) {
                var entityAssociation = collectionAssociations[0];
                var internalParameter = BuildInternalParameter(parameters, entityAssociation, results);
                await FetchAsync(internalParameter, paginatedSearch);
                Log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "Finish Collection Resolving for {0} Collections", entityAssociation.Qualifier));
                return results;
            }
            // multiple compositions being fetched: each in a new Thread
            var tasks = new Task[collectionAssociations.Count];
            var i = 0;
            foreach (var collectionAssociation in collectionAssociations) {
                var internalParameter = BuildInternalParameter(parameters, collectionAssociation, results);
                var perThreadPaginatedSearch = paginatedSearch == null ? null : (PaginatedSearchRequestDto)paginatedSearch.ShallowCopy();
                tasks[i++] = FetchAsync(internalParameter, perThreadPaginatedSearch);
            }
            await Task.WhenAll(tasks);
            Log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "Finish Collection Resolving for {0} Collections", String.Join(",", compositionSchemas.Keys)));
            return results;
        }

        private InternalCollectionResolverParameter BuildInternalParameter(CollectionResolverParameters parameters, EntityAssociation collectionAssociation, IDictionary<string, EntityRepository.EntityRepository.SearchEntityResult> results) {
            var ctx = ContextLookuper.LookupContext();
            var compositionRowstamps = parameters.RowstampMap ?? new Dictionary<string, long?>();
            long? rowstamp = null;
            if (compositionRowstamps.ContainsKey(collectionAssociation.Qualifier)) {
                rowstamp = compositionRowstamps[collectionAssociation.Qualifier];
            }

            var internalParameter = new InternalCollectionResolverParameter {
                ExternalParameters = parameters,
                CollectionAssociation = collectionAssociation,
                Ctx = ctx.ShallowCopy(),
                Results = results,
                Rowstamp = rowstamp
            };
            return internalParameter;
        }


        private async Task FetchAsync(InternalCollectionResolverParameter parameter, PaginatedSearchRequestDto paginatedSearch = null) {

            var entityMetadata = parameter.EntityMetadata;

            Quartz.Util.LogicalThreadContext.SetData("context", parameter.Ctx);
            var collectionAssociation = parameter.CollectionAssociation;

            var collectionEntityMetadata = MetadataProvider.Entity(collectionAssociation.To);
            var targetCollectionAttribute = EntityUtil.GetRelationshipName(collectionAssociation.Qualifier);

            var applicationCompositionSchema = parameter.CompositionSchema;

            var attributeHolders = parameter.EntitiesList as AttributeHolder[] ?? parameter.EntitiesList.ToArray();

            var offLineMode = parameter.Ctx.OfflineMode;

            var matchingResultWrapper = GetResultWrapper();

            var searchRequestDto = BuildSearchRequestDto(parameter, matchingResultWrapper, paginatedSearch);

            searchRequestDto.QueryAlias = collectionAssociation.To;

            var firstAttributeHolder = attributeHolders.First();
            if (applicationCompositionSchema.PrefilterFunction != null) {
                var dataSet = DataSetProvider.GetInstance().LookupDataSet(entityMetadata.ApplicationName, entityMetadata.AppSchema.SchemaId);
                //we will call the function passing the first entry, altough this method could have been invoked for a list of items (printing)
                //TODO: think about it
                var preFilterParam = new CompositionPreFilterFunctionParameters(entityMetadata, searchRequestDto, firstAttributeHolder, applicationCompositionSchema);
                searchRequestDto = PrefilterInvoker.ApplyPreFilterFunction(dataSet, preFilterParam, applicationCompositionSchema.PrefilterFunction);
            }

            EntityRepository.EntityRepository.SearchEntityResult queryResult = null;

            if (paginatedSearch == null) {
                //if there´s no pagination needed we can just do one thread-query
                var dto = searchRequestDto.ShallowCopy();
                queryResult = await EntityRepository.GetAsRawDictionary(collectionEntityMetadata, dto, offLineMode);
            } else {
                // one thread to fetch results
                var ctx = ContextLookuper.LookupContext();
                var tasks = new Task[2];
//                tasks[0] = Task.Factory.NewThread(async c => {
//                    var dto = searchRequestDto.ShallowCopy();
//                    Quartz.Util.LogicalThreadContext.SetData("context", c);
//                    queryResult = await GetList(collectionEntityMetadata, dto, offLineMode);
//                }, ctx);
//                // one thread to count results for paginations
//                tasks[1] = Task.Factory.NewThread(async c => {
//                    var dto = searchRequestDto.ShallowCopy();
//                    Quartz.Util.LogicalThreadContext.SetData("context", c);
//                    paginatedSearch.TotalCount = await GetCount(collectionEntityMetadata, dto, offLineMode);
//                }, ctx);

                var listTask = GetList(collectionEntityMetadata, searchRequestDto, offLineMode);
                var countTask = GetCount(collectionEntityMetadata, searchRequestDto, offLineMode);


                await Task.WhenAll(listTask, countTask);

                queryResult = listTask.Result;
                paginatedSearch.TotalCount = countTask.Result;

                // add paginationData to result 
                // creating a new pagination data in order to have everything calculated correctly
                queryResult.PaginationData = new PaginatedSearchRequestDto(
                    paginatedSearch.TotalCount,
                    paginatedSearch.PageNumber,
                    paginatedSearch.PageSize,
                    paginatedSearch.SearchValues,
                    paginatedSearch.PaginationOptions
                    );
            }

            if (offLineMode) {
                //If on offline mode, we don´t need to match the collections back, we´ll simply return the plain list
                parameter.Results.Add(collectionAssociation.Qualifier, queryResult);
                return;
            }

            if (attributeHolders.Length == 1) {
                //default scenario, we have just one entity here
                if (!firstAttributeHolder.ContainsKey(targetCollectionAttribute)) {
                    firstAttributeHolder.Add(targetCollectionAttribute, queryResult.ResultList);
                } else {
                    var list = (List<Dictionary<string, object>>)firstAttributeHolder[targetCollectionAttribute];
                    list.AddRange(queryResult.ResultList);
                }

                parameter.Results.Add(collectionAssociation.Qualifier, queryResult);
                return;
            }
            MatchResults(queryResult, matchingResultWrapper, targetCollectionAttribute);
        }

        protected virtual async Task<EntityRepository.EntityRepository.SearchEntityResult> GetList(EntityMetadata entityMetadata, SearchRequestDto dto, bool offlineMode) {
            return await EntityRepository.GetAsRawDictionary(entityMetadata, dto, offlineMode);
        }

        protected virtual async Task<int> GetCount(EntityMetadata entityMetadata, SearchRequestDto dto, bool offlineMode) {
            return await EntityRepository.Count(entityMetadata, dto);
        }

        protected virtual CollectionMatchingResultWrapper GetResultWrapper() {
            return new CollectionMatchingResultWrapper();
        }


        public virtual SearchRequestDto BuildSearchRequestDto(InternalCollectionResolverParameter parameter,
            CollectionMatchingResultWrapper matchingResultWrapper, PaginatedSearchRequestDto paginatedSearch = null) {
            var collectionAssociation = parameter.CollectionAssociation;

            var lookupAttributes = collectionAssociation.Attributes;
            SearchRequestDto searchRequestDto;
            if (paginatedSearch != null && (!string.IsNullOrEmpty(paginatedSearch.SearchParams) || !string.IsNullOrEmpty(paginatedSearch.SearchSort))) {
                searchRequestDto = paginatedSearch;
            } else if (paginatedSearch != null && paginatedSearch.PageSize > 0) {
                searchRequestDto = new PaginatedSearchRequestDto();
            } else {
                searchRequestDto = new SearchRequestDto();
            }

            var lookupContext = ContextLookuper.LookupContext();
            var printMode = lookupContext.PrintMode;
            var offLineMode = lookupContext.OfflineMode;

            searchRequestDto.BuildProjection(parameter.CompositionSchema, printMode, offLineMode);

            foreach (var lookupAttribute in lookupAttributes) {
                if (lookupAttribute.From != null) {
                    matchingResultWrapper.AddKey(lookupAttribute.To);

                    BuildParentQueryConstraint(matchingResultWrapper, parameter, lookupAttribute, searchRequestDto, collectionAssociation.To);
                } else if (lookupAttribute.Literal != null) {
                    //if the from is a literal, don´t bother with the entities values
                    searchRequestDto.AppendSearchEntry(lookupAttribute.To, lookupAttribute.Literal);
                } else if (lookupAttribute.Query != null) {
                    searchRequestDto.AppendWhereClause(lookupAttribute.GetQueryReplacingMarkers(parameter.EntityMetadata.Name));
                }
            }

            var orderByField = parameter.CompositionSchema.CollectionProperties.OrderByField;
            if (orderByField != null && string.IsNullOrEmpty(searchRequestDto.SearchSort)) {
                searchRequestDto.SearchSort = orderByField;
                searchRequestDto.SearchAscending = !orderByField.EndsWith("desc");
            }
            // no pagination intended: return simple search
            var paginatedDTO = searchRequestDto as PaginatedSearchRequestDto;
            if (paginatedDTO == null || paginatedSearch == null) {
                return searchRequestDto;
            }

            // pagination: merging the search dto's
            paginatedDTO.PageNumber = paginatedSearch.PageNumber;
            paginatedDTO.PageSize = paginatedSearch.PageSize;
            paginatedDTO.TotalCount = paginatedSearch.TotalCount;
            return searchRequestDto;
        }

        protected virtual void BuildParentQueryConstraint(CollectionMatchingResultWrapper matchingResultWrapper, InternalCollectionResolverParameter parameter, EntityAssociationAttribute lookupAttribute,
            SearchRequestDto searchRequestDto, string relationshipName) {
            var searchValues = new HashSet<string>();
            var attributeHolders = parameter.EntitiesList;
            var enumerable = attributeHolders as AttributeHolder[] ?? attributeHolders.ToArray();
            var hasMainEntity = enumerable.Any();
            foreach (var entity in enumerable) {
                var key = matchingResultWrapper.FetchKey(entity);
                var searchValue = SearchUtils.GetSearchValue(lookupAttribute, entity);
                if (!String.IsNullOrWhiteSpace(searchValue)) {
                    searchValues.Add(searchValue);
                    if (lookupAttribute.To != null) {
                        key.AppendEntry(lookupAttribute.To, searchValue);
                    }
                }
            }
            if (searchValues.Any()) {
                if (lookupAttribute.To != null) {
                    searchRequestDto.AppendSearchEntry(lookupAttribute.To, searchValues, lookupAttribute.AllowsNull);
                } else if (lookupAttribute.Query != null) {
                    //TODO: support for multiple entities on print
                    searchRequestDto.AppendWhereClause(lookupAttribute.GetQueryReplacingMarkers(parameter.EntityMetadata.Name, searchValues.FirstOrDefault()));
                }
            } else if (hasMainEntity && lookupAttribute.Primary) {
                //if nothing was provided, it should return nothing, instead of all the values --> 
                //if the main entity had a null on a primary element of the composition, nothing should be seen
                searchRequestDto.AppendSearchEntry(lookupAttribute.To, new[] { "-1231231312" });
            }
        }

        private void MatchResults(EntityRepository.EntityRepository.SearchEntityResult resultCollections,
            CollectionMatchingResultWrapper matchingResultWrapper, string targetCollectionAttribute) {
            foreach (var resultCollection in resultCollections.ResultList) {
                var resultkey = new CollectionMatchingResultKey();
                foreach (var key in matchingResultWrapper.Keys) {
                    var result = resultCollection[key];
                    if (result != null) {
                        resultkey.AppendEntry(key, result.ToString());
                    }
                }
                var foundEntity = matchingResultWrapper.FetchEntity(resultkey);
                var attributes = foundEntity;
                if (!attributes.ContainsKey(targetCollectionAttribute)) {
                    attributes.Add(targetCollectionAttribute, new List<IDictionary<string, object>>());
                }
                var collection = (List<IDictionary<string, object>>)attributes[targetCollectionAttribute];
                collection.Add(resultCollection);
            }
        }

        #region matching Helper classes

        public class CollectionMatchingResultWrapper {

            readonly IDictionary<AttributeHolder, CollectionMatchingResultKey> _inverseDict = new Dictionary<AttributeHolder, CollectionMatchingResultKey>();

            readonly IDictionary<CollectionMatchingResultKey, AttributeHolder> _matchingDict = new Dictionary<CollectionMatchingResultKey, AttributeHolder>();
            internal readonly ISet<string> Keys = new HashSet<string>();

            internal void AddKey(string key) {
                Keys.Add(key);
            }

            public virtual CollectionMatchingResultKey FetchKey(AttributeHolder entity) {
                CollectionMatchingResultKey key;
                if (!_inverseDict.TryGetValue(entity, out key)) {
                    key = new CollectionMatchingResultKey();
                    _matchingDict[key] = entity;
                    _inverseDict[entity] = key;
                }
                return key;
            }

            internal AttributeHolder FetchEntity(CollectionMatchingResultKey key) {
                return _matchingDict[key];
            }
        }


        public class CollectionMatchingResultKey {
            //holds for each attribute used in the relationship the value, so it can be matched later
            readonly IDictionary<string, string> _pairs = new Dictionary<string, string>();

            protected bool Equals(CollectionMatchingResultKey other) {
                return other._pairs.OrderBy(r => r.Key).SequenceEqual(_pairs.OrderBy(r => r.Key));
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != GetType())
                    return false;
                return Equals((CollectionMatchingResultKey)obj);
            }

            public override int GetHashCode() {
                return 0;
            }

            internal void AppendEntry(string key, string value) {
                _pairs[key] = value;
            }

            public override string ToString() {
                return string.Format("Pairs: {0}", String.Join(",", _pairs.Keys));
            }
        }
        #endregion

    }
}
