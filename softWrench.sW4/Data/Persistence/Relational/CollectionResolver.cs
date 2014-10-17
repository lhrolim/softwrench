﻿using log4net;
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
    public class CollectionResolver {

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

        public void ResolveCollections(SlicedEntityMetadata entityMetadata, IDictionary<string, ApplicationCompositionSchema> compositionSchemas,
            AttributeHolder mainEntity) {
            ResolveCollections(entityMetadata, compositionSchemas, new List<AttributeHolder> { mainEntity });
        }



        public void ResolveCollections(SlicedEntityMetadata entityMetadata, IDictionary<string, ApplicationCompositionSchema> compositionSchemas,
            IReadOnlyList<AttributeHolder> entitiesList) {
            if (!compositionSchemas.Any()) {
                return;
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
            foreach (var collectionAssociation in collectionAssociations) {
                var association = collectionAssociation;
                tasks[i++] = Task.Factory.StartNew(() => FetchAsync(association, compositionSchemas, entitiesList, ctx));
            }
            Task.WaitAll(tasks);
            _log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "Finish Collection Resolving for {0} Collections",
                String.Join(",", compositionSchemas.Keys)));
        }


        private void FetchAsync(EntityAssociation collectionAssociation, IDictionary<string, ApplicationCompositionSchema> compositionSchemas,
            IEnumerable<AttributeHolder> entitiesList, ContextHolder ctx) {
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

            var searchRequestDto = BuildSearchRequestDto(applicationCompositionSchema, lookupattributes, matchingResultWrapper, attributeHolders, collectionEntityMetadata);

            var listOfCollections = EntityRepository.GetAsRawDictionary(collectionEntityMetadata, searchRequestDto);
            if (attributeHolders.Count() == 1) {
                //default scenario, we have just one entity here
                attributeHolders.First().Attributes.Add(targetCollectionAttribute, listOfCollections);
                return;
            }
            MatchResults(listOfCollections, matchingResultWrapper, targetCollectionAttribute);
        }

        private static SearchRequestDto BuildSearchRequestDto(ApplicationCompositionCollectionSchema applicationCompositionSchema,
            IEnumerable<EntityAssociationAttribute> lookupattributes, CollectionMatchingResultWrapper matchingResultWrapper,
            AttributeHolder[] attributeHolders, EntityMetadata collectionEntityMetadata) {

            var searchRequestDto = new SearchRequestDto();
            searchRequestDto.BuildProjection(applicationCompositionSchema);

            foreach (var lookupAttribute in lookupattributes) {
                if (lookupAttribute.From != null) {
                    matchingResultWrapper.AddKey(lookupAttribute.To);
                    var searchValues = new List<string>();
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
            return searchRequestDto;
        }

        private void MatchResults(IEnumerable<IDictionary<string, object>> resultCollections, CollectionMatchingResultWrapper matchingResultWrapper, string targetCollectionAttribute) {
            foreach (var resultCollection in resultCollections) {
                var resultkey = new CollectionMatchingResultKey();
                foreach (var key in matchingResultWrapper.Keys) {
                    var result = resultCollection[key];
                    if (result != null) {
                        resultkey.AppendEntry(key, result.ToString());
                    }
                }
                var attributes = matchingResultWrapper.FetchEntity(resultkey).Attributes;
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

            internal AttributeHolder FetchEntity(CollectionMatchingResultKey key) {
                return _matchingDict[key];
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
