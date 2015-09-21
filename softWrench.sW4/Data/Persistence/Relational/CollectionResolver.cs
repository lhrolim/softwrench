using log4net;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Security.Context;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.Persistence.Relational {
    public class CollectionResolver : ISingletonComponent {


        private readonly ILog _log = LogManager.GetLogger(typeof(CollectionResolver));

        private IContextLookuper _contextLookuper;

        private EntityRepository _repository;

        private EntityRepository EntityRepository {
            get {
                if (_repository == null) {
                    _repository =
                        SimpleInjectorGenericFactory.Instance.GetObject<EntityRepository>(typeof(EntityRepository));
                }
                return _repository;
            }
        }


        protected IContextLookuper ContextLookuper {
            get {
                if (_contextLookuper != null) {
                    return _contextLookuper;
                }
                _contextLookuper =
                    SimpleInjectorGenericFactory.Instance.GetObject<IContextLookuper>(typeof(IContextLookuper));
                return _contextLookuper;
            }
        }

        public Dictionary<string, EntityRepository.SearchEntityResult> ResolveCollections(SlicedEntityMetadata entityMetadata, IDictionary<string, ApplicationCompositionSchema> compositionSchemas,
            AttributeHolder mainEntity, PaginatedSearchRequestDto paginatedSearch = null) {
            return ResolveCollections(entityMetadata, compositionSchemas, new List<AttributeHolder>() { mainEntity },paginatedSearch);
        }

        public Dictionary<string, EntityRepository.SearchEntityResult> ResolveCollections(SlicedEntityMetadata entityMetadata, IDictionary<string, ApplicationCompositionSchema> compositionSchemas,
            IReadOnlyList<AttributeHolder> entitiesList, PaginatedSearchRequestDto paginatedSearch = null) {
            if (!compositionSchemas.Any()) {
                return new Dictionary<string, EntityRepository.SearchEntityResult>();
            }
            var before = Stopwatch.StartNew();
            _log.DebugFormat("Init Collection Resolving for {0} Collections", String.Join(",", compositionSchemas.Keys));

            var collectionAssociations = new List<EntityAssociation>();
            foreach (var entityListAssociation in entityMetadata.ListAssociations()) {
                if (compositionSchemas.Keys.Contains(entityListAssociation.Qualifier)) {
                    collectionAssociations.Add(entityListAssociation);
                }
            }

            var results = new Dictionary<string, EntityRepository.SearchEntityResult>();

            var tasks = new Task[collectionAssociations.Count];
            var i = 0;
            var ctx = ContextLookuper.LookupContext();
            foreach (var collectionAssociation in collectionAssociations) {
                var association = collectionAssociation;
                var perThreadPaginatedSearch = paginatedSearch == null ? null : (PaginatedSearchRequestDto)paginatedSearch.ShallowCopy();
                //this will avoid that one thread impacts any other
                var perThreadContext = ctx.ShallowCopy();
                tasks[i++] = Task.Factory.NewThread(() => FetchAsync(entityMetadata, association, compositionSchemas, entitiesList, perThreadContext, results, perThreadPaginatedSearch));
            }
            Task.WaitAll(tasks);
            _log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "Finish Collection Resolving for {0} Collections",
                String.Join(",", compositionSchemas.Keys)));


            return results;

        }


        private void FetchAsync(SlicedEntityMetadata entityMetadata, EntityAssociation collectionAssociation, IDictionary<string, ApplicationCompositionSchema> compositionSchemas, IEnumerable<AttributeHolder> entitiesList,
            ContextHolder ctx, Dictionary<string, EntityRepository.SearchEntityResult> results, PaginatedSearchRequestDto paginatedSearch) {
            Quartz.Util.LogicalThreadContext.SetData("context", ctx);
            var lookupAttributes = collectionAssociation.Attributes;
            var collectionEntityMetadata = MetadataProvider.Entity(collectionAssociation.To);
            var targetCollectionAttribute = EntityUtil.GetRelationshipName(collectionAssociation.Qualifier);
            var applicationCompositionSchema = compositionSchemas[collectionAssociation.Qualifier] as ApplicationCompositionCollectionSchema;
            if (applicationCompositionSchema == null) {
                throw ExceptionUtil.InvalidOperation("collection schema {0} not found", collectionAssociation.Qualifier);
            }


            var lookupattributes = lookupAttributes as EntityAssociationAttribute[] ?? lookupAttributes.ToArray();
            var attributeHolders = entitiesList as AttributeHolder[] ?? entitiesList.ToArray();
            var matchingResultWrapper = new CollectionMatchingResultWrapper();

            var searchRequestDto = BuildSearchRequestDto(applicationCompositionSchema, lookupattributes, matchingResultWrapper, attributeHolders, collectionEntityMetadata, paginatedSearch);
            searchRequestDto.QueryAlias = collectionAssociation.To;

            var firstAttributeHolder = attributeHolders.First();
            if (applicationCompositionSchema.PrefilterFunction != null) {
                var dataSet = DataSetProvider.GetInstance().LookupDataSet(entityMetadata.ApplicationName);
                //we will call the function passing the first entry, altough this method could have been invoked for a list of items (printing)
                //TODO: think about it
                var preFilterParam = new CompositionPreFilterFunctionParameters(entityMetadata.AppSchema, searchRequestDto, firstAttributeHolder, applicationCompositionSchema);
                searchRequestDto = PrefilterInvoker.ApplyPreFilterFunction(dataSet, preFilterParam, applicationCompositionSchema.PrefilterFunction);
            }

            EntityRepository.SearchEntityResult queryResult = null;
            // one thread to fetch results
            var tasks = new Task[2];
            tasks[0] = Task.Factory.NewThread(() => {
                queryResult = EntityRepository.GetAsRawDictionary(collectionEntityMetadata, searchRequestDto);
            });
            // one thread to count results for paginations
            tasks[1] = Task.Factory.NewThread(() => {
                if (paginatedSearch != null) {
                    paginatedSearch.TotalCount = EntityRepository.Count(collectionEntityMetadata, searchRequestDto);
                }
            });
            Task.WaitAll(tasks);
            // add paginationData to result 
            if (paginatedSearch != null) {
                // creating a new pagination data in order to have everything calculated correctly
                queryResult.PaginationData = new PaginatedSearchRequestDto(
                    paginatedSearch.TotalCount,
                    paginatedSearch.PageNumber,
                    paginatedSearch.PageSize,
                    paginatedSearch.SearchValues,
                    paginatedSearch.PaginationOptions
                    );
            }

            var listOfCollections = _repository.GetAsRawDictionary(collectionEntityMetadata, searchRequestDto);
            if (attributeHolders.Count() == 1) {
                //default scenario, we have just one entity here
                results.Add(collectionAssociation.Qualifier, queryResult);
                firstAttributeHolder.Attributes.Add(targetCollectionAttribute, listOfCollections.ResultList);
                return;
            }
            MatchResults(listOfCollections, matchingResultWrapper, targetCollectionAttribute);
        }

        private SearchRequestDto BuildSearchRequestDto(ApplicationCompositionCollectionSchema applicationCompositionSchema, IEnumerable<EntityAssociationAttribute> lookupattributes, 
            CollectionMatchingResultWrapper matchingResultWrapper, AttributeHolder[] attributeHolders, EntityMetadata collectionEntityMetadata, PaginatedSearchRequestDto paginatedSearch) {

            var searchRequestDto = new PaginatedSearchRequestDto();

            searchRequestDto.BuildProjection(applicationCompositionSchema, ContextLookuper.LookupContext().PrintMode);

            foreach (var lookupAttribute in lookupattributes) {
                if (lookupAttribute.From != null) {
                    matchingResultWrapper.AddKey(lookupAttribute.To);
                    BuildParentQueryConstraint(matchingResultWrapper, attributeHolders, lookupAttribute, searchRequestDto);
                } else if (lookupAttribute.Literal != null) {
                    //if the from is a literal, don´t bother with the entities values
                    searchRequestDto.AppendSearchEntry(lookupAttribute.To, lookupAttribute.Literal);
                } else if (lookupAttribute.Query != null) {
                    searchRequestDto.AppendWhereClause(lookupAttribute.GetQueryReplacingMarkers(collectionEntityMetadata.Name));
                }
            }

            var orderByField = applicationCompositionSchema.CollectionProperties.OrderByField;
            if (orderByField != null) {
                searchRequestDto.SearchSort = orderByField;
                searchRequestDto.SearchAscending = !orderByField.EndsWith("desc");
            }
            // no pagination intended: return simple search
            if (paginatedSearch == null || paginatedSearch.PageSize <= 0) {
                return searchRequestDto;
            }
            // pagination: merging the search dto's
            searchRequestDto.PageNumber = paginatedSearch.PageNumber;
            searchRequestDto.PageSize = paginatedSearch.PageSize;
            searchRequestDto.TotalCount = paginatedSearch.TotalCount;
            return searchRequestDto;
        }

        private static void BuildParentQueryConstraint(CollectionMatchingResultWrapper matchingResultWrapper,
            AttributeHolder[] attributeHolders, EntityAssociationAttribute lookupAttribute,
            PaginatedSearchRequestDto searchRequestDto) {
            var searchValues = new List<string>();
            var enumerable = attributeHolders as AttributeHolder[] ?? attributeHolders.ToArray();
            var hasMainEntity = enumerable.Any();
            foreach (var entity in attributeHolders) {
                var key = matchingResultWrapper.FetchKey(entity);
                var searchValue = SearchUtils.GetSearchValue(lookupAttribute, entity);
                if (!String.IsNullOrWhiteSpace(searchValue) && lookupAttribute.To != null) {
                    searchValues.Add(searchValue);
                    key.AppendEntry(lookupAttribute.To, searchValue);
                }
            }
            if (searchValues.Any()) {
                searchRequestDto.AppendSearchEntry(lookupAttribute.To, searchValues);
            } else if (hasMainEntity && lookupAttribute.Primary) {
                //if nothing was provided, it should return nothing, instead of all the values --> 
                //if the main entity had a null on a primary element of the composition, nothing should be seen
                searchRequestDto.AppendSearchEntry(lookupAttribute.To, new[] { "-1231231312" });
            }
        }

        private void MatchResults(EntityRepository.SearchEntityResult resultCollections, CollectionMatchingResultWrapper matchingResultWrapper, string targetCollectionAttribute) {
            foreach (var resultCollection in resultCollections.ResultList) {
                var resultkey = new CollectionMatchingResultKey();
                foreach (var key in matchingResultWrapper.Keys) {
                    object result;
                    if (!resultCollection.TryGetValue(key, out result)) {
                        throw new Exception("key {0} was not present on the dictionary".Fmt(key));
                    }
                    if (result != null) {
                        resultkey.AppendEntry(key, result.ToString());
                    }
                }

                IDictionary<string, object> attributes;

                if (targetCollectionAttribute == "worklog_" && resultCollection.ContainsKey("relatedrecordkey") && resultCollection["relatedrecordkey"] != null) {
                    //let´s see if this was provenient from a related record, workaround for //HAP-968
                    // see also HapagBaseApplicationDataSet#AppendRelatedRecordWCToWorklog
                    resultkey = new CollectionMatchingResultKey();
                    resultkey.AppendEntry("recordkey", resultCollection["relatedrecordkey"] as string);
                    attributes = matchingResultWrapper.FetchEntity(resultkey);
                    if (attributes == null) {
                        throw new Exception("could not locate entry");
                    }
                } else {
                    attributes = matchingResultWrapper.FetchEntity(resultkey);
                }




                if (!attributes.ContainsKey(targetCollectionAttribute)) {
                    attributes.Add(targetCollectionAttribute, new List<IDictionary<string, object>>());
                }
                var collection = (List<IDictionary<string, object>>)attributes[targetCollectionAttribute];
                collection.Add(resultCollection);
            }
        }

        #region matching Helper classes

        class CollectionMatchingResultWrapper {

            readonly IDictionary<AttributeHolder, CollectionMatchingResultKey> _inverseDict = new Dictionary<AttributeHolder, CollectionMatchingResultKey>();

            readonly IDictionary<CollectionMatchingResultKey, AttributeHolder> _matchingDict = new Dictionary<CollectionMatchingResultKey, AttributeHolder>();
            internal readonly ISet<string> Keys = new HashSet<string>();

            internal void AddKey(string key) {
                Keys.Add(key);
            }

            internal void AppendEntry(CollectionMatchingResultKey key, AttributeHolder value) {
                _matchingDict[key] = value;
            }

            internal CollectionMatchingResultKey FetchKey(AttributeHolder entity) {
                CollectionMatchingResultKey key;
                if (!_inverseDict.TryGetValue(entity, out key)) {
                    key = new CollectionMatchingResultKey();
                    _matchingDict[key] = entity;
                    _inverseDict[entity] = key;
                }
                return key;
            }

            internal IDictionary<string, object> FetchEntity(CollectionMatchingResultKey key) {
                AttributeHolder result;
                if (!_matchingDict.TryGetValue(key, out result)) {
                    return null;
                }
                return result.Attributes;
            }
        }

        class CollectionMatchingResultKey {
            //holds for each attribute used in the relationship the value, so it can be matched later
            readonly IDictionary<string, string> _pairs = new Dictionary<string, string>();

            protected bool Equals(CollectionMatchingResultKey other) {
                return other._pairs.OrderBy(r => r.Key).SequenceEqual(_pairs.OrderBy(r => r.Key));
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
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
