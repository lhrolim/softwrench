using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.Util;
using log4net;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Security.Context;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.Collection {
    public class CollectionResolver : ISingletonComponent {

        private EntityRepository.EntityRepository _repository;

        private EntityRepository.EntityRepository EntityRepository {
            get {
                if (_repository == null) {
                    _repository =
                        SimpleInjectorGenericFactory.Instance.GetObject<EntityRepository.EntityRepository>(typeof(EntityRepository.EntityRepository));
                }
                return _repository;
            }
        }

        private readonly ILog _log = LogManager.GetLogger(typeof(CollectionResolver));

        private IContextLookuper _contextLookuper;

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


        public void ResolveCollections(SlicedEntityMetadata entityMetadata, IDictionary<string, ApplicationCompositionSchema>
            compositionSchemas, IEnumerable<AttributeHolder> attributeHolders) {
            DoResolveCollections(new CollectionResolverParameters(compositionSchemas, entityMetadata, attributeHolders));
        }

        public void ResolveCollections(SlicedEntityMetadata entityMetadata, IDictionary<string, ApplicationCompositionSchema>
            compositionSchemas, AttributeHolder attributeHolders) {
            DoResolveCollections(new CollectionResolverParameters(compositionSchemas, entityMetadata, new List<AttributeHolder> { attributeHolders }));
        }


        public Dictionary<string, EntityRepository.EntityRepository.SearchEntityResult> ResolveCollections(CollectionResolverParameters parameters) {
            return DoResolveCollections(parameters);
        }

        private Dictionary<string, EntityRepository.EntityRepository.SearchEntityResult> DoResolveCollections(CollectionResolverParameters parameters) {
            var compositionSchemas = parameters.CompositionSchemas;
            var compositionRowstamps = parameters.RowstampMap;
            var entityMetadata = parameters.SlicedEntity;

            if (!compositionSchemas.Any()) {
                return new Dictionary<string, EntityRepository.EntityRepository.SearchEntityResult>();
            }
            if (compositionRowstamps == null) {
                compositionRowstamps = new Dictionary<string, long?>();
            }

            var before = Stopwatch.StartNew();
            _log.DebugFormat("Init Collection Resolving for {0} Collections", String.Join(",", compositionSchemas.Keys));

            var collectionAssociations = new List<EntityAssociation>();
            foreach (var entityListAssociation in entityMetadata.ListAssociations()) {
                if (compositionSchemas.Keys.Contains(entityListAssociation.Qualifier)) {
                    collectionAssociations.Add(entityListAssociation);
                }
            }
            var tasks = new Task[collectionAssociations.Count];
            var i = 0;
            var ctx = ContextLookuper.LookupContext();
            var results = new Dictionary<string, EntityRepository.EntityRepository.SearchEntityResult>();
            foreach (var collectionAssociation in collectionAssociations) {
                long? rowstamp = null;
                if (compositionRowstamps.ContainsKey(collectionAssociation.Qualifier)) {
                    rowstamp = compositionRowstamps[collectionAssociation.Qualifier];
                }

                var internalParameter = new InternalCollectionResolverParameter {
                    ExternalParameters = parameters,
                    CollectionAssociation = collectionAssociation,
                    Ctx = ctx,
                    Results = results,
                    Rowstamp = rowstamp
                };
                tasks[i++] = Task.Factory.NewThread(() => FetchAsync(internalParameter));
            }
            Task.WaitAll(tasks);
            _log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "Finish Collection Resolving for {0} Collections",
                String.Join(",", compositionSchemas.Keys)));
            return results;
        }


        private void FetchAsync(InternalCollectionResolverParameter parameter) {

            var entityMetadata = parameter.EntityMetadata;

            Quartz.Util.LogicalThreadContext.SetData("context", parameter.Ctx);
            var collectionAssociation = parameter.CollectionAssociation;

            var collectionEntityMetadata = MetadataProvider.Entity(collectionAssociation.To);
            var targetCollectionAttribute = EntityUtil.GetRelationshipName(collectionAssociation.Qualifier);

            var applicationCompositionSchema = parameter.CompositionSchema;

            var attributeHolders = parameter.EntitiesList as AttributeHolder[] ?? parameter.EntitiesList.ToArray();

            var offLineMode = parameter.Ctx.OfflineMode;



            var matchingResultWrapper = GetResultWrapper();

            var searchRequestDto = BuildSearchRequestDto(parameter, matchingResultWrapper);

            var firstAttributeHolder = attributeHolders.First();
            if (applicationCompositionSchema.PrefilterFunction != null) {
                var dataSet = DataSetProvider.GetInstance().LookupDataSet(entityMetadata.ApplicationName, entityMetadata.AppSchema.SchemaId);
                //we will call the function passing the first entry, altough this method could have been invoked for a list of items (printing)
                //TODO: think about it
                var preFilterParam = new CompositionPreFilterFunctionParameters(entityMetadata.Schema, searchRequestDto,
                    firstAttributeHolder, applicationCompositionSchema);
                searchRequestDto = PrefilterInvoker.ApplyPreFilterFunction(dataSet, preFilterParam, applicationCompositionSchema.PrefilterFunction);
            }


            var queryResult = EntityRepository.GetAsRawDictionary(collectionEntityMetadata, searchRequestDto, offLineMode);

            if (offLineMode) {
                //If on offline mode, we don´t need to match the collections back, we´ll simply return the plain list
                parameter.Results.Add(collectionAssociation.Qualifier, queryResult);
                return;
            }


            if (attributeHolders.Count() == 1) {
                //default scenario, we have just one entity here
                firstAttributeHolder.Attributes.Add(targetCollectionAttribute, queryResult.ResultList);
                return;
            }
            MatchResults(queryResult, matchingResultWrapper, targetCollectionAttribute);
        }

        protected virtual CollectionMatchingResultWrapper GetResultWrapper() {
            return new CollectionMatchingResultWrapper();
        }


        protected virtual SearchRequestDto BuildSearchRequestDto(InternalCollectionResolverParameter parameter,
            CollectionMatchingResultWrapper matchingResultWrapper) {
            var collectionAssociation = parameter.CollectionAssociation;


            var lookupAttributes = collectionAssociation.Attributes;
            var searchRequestDto = new SearchRequestDto();

            var lookupContext = ContextLookuper.LookupContext();
            var printMode = lookupContext.PrintMode;
            var offLineMode = lookupContext.OfflineMode;

            searchRequestDto.BuildProjection(parameter.CompositionSchema, printMode, offLineMode);

            foreach (var lookupAttribute in lookupAttributes) {
                if (lookupAttribute.From != null) {
                    matchingResultWrapper.AddKey(lookupAttribute.To);

                    BuildParentQueryConstraint(matchingResultWrapper, parameter, lookupAttribute, searchRequestDto);
                } else if (lookupAttribute.Literal != null) {
                    //if the from is a literal, don´t bother with the entities values
                    searchRequestDto.AppendSearchEntry(lookupAttribute.To, lookupAttribute.Literal);
                } else if (lookupAttribute.Query != null) {
                    searchRequestDto.AppendWhereClause(lookupAttribute.GetQueryReplacingMarkers(parameter.EntityMetadata.Name));
                }
            }

            var orderByField = parameter.CompositionSchema.CollectionProperties.OrderByField;
            if (orderByField != null) {
                searchRequestDto.SearchSort = orderByField;
                searchRequestDto.SearchAscending = !orderByField.EndsWith("desc");
            }
            return searchRequestDto;
        }

        protected virtual void BuildParentQueryConstraint(CollectionMatchingResultWrapper matchingResultWrapper, InternalCollectionResolverParameter parameter, EntityAssociationAttribute lookupAttribute,
            SearchRequestDto searchRequestDto) {
            var searchValues = new HashSet<string>();
            var attributeHolders = parameter.EntitiesList;
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
                var attributes = foundEntity.Attributes;
                if (!attributes.ContainsKey(targetCollectionAttribute)) {
                    attributes.Add(targetCollectionAttribute, new List<IDictionary<string, object>>());
                }
                var collection = (List<IDictionary<string, object>>)attributes[targetCollectionAttribute];
                collection.Add(resultCollection);
            }
        }

        #region matching Helper classes

        protected class CollectionMatchingResultWrapper {

            readonly IDictionary<AttributeHolder, CollectionMatchingResultKey> _inverseDict = new Dictionary<AttributeHolder, CollectionMatchingResultKey>();

            readonly IDictionary<CollectionMatchingResultKey, AttributeHolder> _matchingDict = new Dictionary<CollectionMatchingResultKey, AttributeHolder>();
            internal readonly ISet<string> Keys = new HashSet<string>();

            internal void AddKey(string key) {
                Keys.Add(key);
            }

            virtual internal CollectionMatchingResultKey FetchKey(AttributeHolder entity) {
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




        protected class CollectionMatchingResultKey {
            //holds for each attribute used in the relationship the value, so it can be matched later
            readonly IDictionary<string, string> _pairs = new Dictionary<string, string>();

            protected bool Equals(CollectionMatchingResultKey other) {
                return other._pairs.OrderBy(r => r.Key).SequenceEqual(_pairs.OrderBy(r => r.Key));
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
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
