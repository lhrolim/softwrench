using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Persistence.Relational;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Security.Context;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    public class BaseApplicationDataSet : IDataSet {

        /// <summary>
        /// if this constant is present we´re interested in getting all the eagerassociations of the schema, rather then a specific association dependency
        /// </summary>
        private const string EagerAssociationTrigger = "#eagerassociations";

        protected static readonly ILog Log = LogManager.GetLogger(typeof(BaseApplicationDataSet));

        //        protected readonly MaximoConnectorEngine _maximoConnectorEngine;

        private readonly ApplicationAssociationResolver _associationOptionResolver = new ApplicationAssociationResolver();
        private readonly DynamicOptionFieldResolver _dynamicOptionFieldResolver = new DynamicOptionFieldResolver();

        private IContextLookuper _contextLookuper;

        private readonly CollectionResolver _collectionResolver = new CollectionResolver();


        protected MaximoConnectorEngine _maximoConnectorEngine {
            get {
                return
                    SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(
                        typeof(MaximoConnectorEngine));
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




        public Int32 GetCount(ApplicationMetadata application, [CanBeNull]IDataRequest request) {


            SearchRequestDto searchDto = null;
            if (request is DataRequestAdapter && request != null) {
                searchDto = ((DataRequestAdapter)request).SearchDTO;
            } else if (request is SearchRequestDto) {
                searchDto = (SearchRequestDto)request;
            }
            searchDto = searchDto ?? new SearchRequestDto();

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            searchDto.BuildProjection(application.Schema);

            return _maximoConnectorEngine.Count(entityMetadata, searchDto);
        }

        public IApplicationResponse Get(ApplicationMetadata application, InMemoryUser user, IDataRequest request) {
            var adapter = request as DataRequestAdapter;
            if (adapter != null) {
                if (adapter.SearchDTO != null) {
                    request = adapter.SearchDTO;
                } else if (adapter.Id != null) {
                    request = DetailRequest.GetInstance(application, adapter);
                }
            }

            if (request is PaginatedSearchRequestDto) {
                return GetList(application, (PaginatedSearchRequestDto)request);
            }
            if (request is DetailRequest) {
                try {
                    return GetApplicationDetail(application, user, (DetailRequest)request);
                } catch (InvalidOperationException e) {
                    return new ApplicationDetailResult(null, null, application.Schema, null, ((DetailRequest)request).Id);
                }

            }
            if (application.Schema.Stereotype == SchemaStereotype.List) {
                return GetList(application, PaginatedSearchRequestDto.DefaultInstance(application.Schema));
            }
            if (application.Schema.Stereotype == SchemaStereotype.Detail || request.Key.Mode == SchemaMode.input) {
                //case of detail of new items
                return GetApplicationDetail(application, user, DetailRequest.GetInstance(application, adapter));
            }
            throw new InvalidOperationException("could not determine which operation to take upon request");
        }


        protected virtual ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var id = request.Id;
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            var applicationCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(application.Schema);
            DataMap datamap;
            if (id == null) {
                datamap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application, request.InitialValues);
            } else {
                datamap =
                    (DataMap)
                        _maximoConnectorEngine.FindById(application.Schema, entityMetadata, id);
                if (datamap == null) {
                    throw new InvalidOperationException("You don´t have enough permissions to see that register. contact your administrator");
                }

                var prefetchCompositions = "true".EqualsIc(application.Schema.GetProperty(ApplicationSchemaPropertiesCatalog.PreFetchCompositions)) || "#all".Equals(request.CompositionsToFetch);
                var compostionsToUse = new Dictionary<string, ApplicationCompositionSchema>();
                foreach (var compositionEntry in applicationCompositionSchemas) {
                    if (prefetchCompositions || FetchType.Eager.Equals(compositionEntry.Value.FetchType) || compositionEntry.Value.INLINE) {
                        compostionsToUse.Add(compositionEntry.Key, compositionEntry.Value);
                    }
                }
                if (compostionsToUse.Any()) {
                    GetCompositionData(application, new PreFetchedCompositionFetchRequest(new List<AttributeHolder> { datamap }), null);
                }
            }

            var associationResults = BuildAssociationOptions(datamap, application, request);
            var detailResult = new ApplicationDetailResult(datamap, associationResults, application.Schema, applicationCompositionSchemas, id);
            return detailResult;
        }

        public virtual CompositionFetchResult GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request, JObject currentData) {

            var applicationCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(application.Schema);
            var compostionsToUse = new Dictionary<string, ApplicationCompositionSchema>();

            if (request.CompositionList != null) {
                foreach (var compositionKey in request.CompositionList.Where(applicationCompositionSchemas.ContainsKey)) {
                    //use only the passed compositions amongst the original list
                    compostionsToUse.Add(compositionKey, applicationCompositionSchemas[compositionKey]);
                }
            } else {
                //use them all
                compostionsToUse = (Dictionary<string, ApplicationCompositionSchema>)applicationCompositionSchemas;
            }

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);

            Dictionary<string, EntityRepository.SearchEntityResult> result;
            if (request is PreFetchedCompositionFetchRequest) {
                //this might be a list or either a single entity, depending whether it´s coming from a list report or from the after save operation
                var listOfEntities = ((PreFetchedCompositionFetchRequest)request).PrefetchEntities;
                result = _collectionResolver.ResolveCollections(entityMetadata, compostionsToUse, listOfEntities);
                return new CompositionFetchResult(result, listOfEntities.FirstOrDefault());
            }
            var cruddata = EntityBuilder.BuildFromJson<Entity>(typeof(Entity), entityMetadata,
               application, currentData, request.Id);
            result = _collectionResolver.ResolveCollections(entityMetadata, compostionsToUse, cruddata, request.PaginatedSearch);
            return new CompositionFetchResult(result, cruddata);
        }




        protected virtual ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var totalCount = searchDto.TotalCount;
            IReadOnlyList<AttributeHolder> entities = null;

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            var schema = application.Schema;
            searchDto.BuildProjection(schema);
            if (searchDto.Context != null && searchDto.Context.MetadataId != null) {
                searchDto.QueryAlias = searchDto.Context.MetadataId;
            } else {
                searchDto.QueryAlias = application.Name + "." + schema.SchemaId;
            }
            if (application.Schema.GetProperty("list.querygeneratorservice") != null) {
                searchDto.QueryGeneratorService = application.Schema.GetProperty("list.querygeneratorservice");
            }


            if (schema.UnionSchema != null) {
                searchDto.BuildUnionDTO(searchDto, schema);
            }
            var propertyValue = schema.GetProperty(ApplicationSchemaPropertiesCatalog.ListSchemaOrderBy);
            if (searchDto.SearchSort == null && propertyValue != null) {
                //if the schema has a default sort defined, and we didn´t especifally asked for any sort column, apply the default schema
                searchDto.SearchSort = propertyValue;
            }

            var tasks = new Task[2];
            var ctx = ContextLookuper.LookupContext();

            //count query
            tasks[0] = Task.Factory.NewThread(c => {
                Quartz.Util.LogicalThreadContext.SetData("context", c);
                if (searchDto.NeedsCountUpdate) {
                    Log.DebugFormat("BaseApplicationDataSet#GetList calling Count method on maximo engine. Application Schema \"{0}\" / Context \"{1}\"", schema, c);
                    totalCount = _maximoConnectorEngine.Count(entityMetadata, searchDto);
                }
            }, ctx);

            //query
            tasks[1] = Task.Factory.NewThread(c => {
                Quartz.Util.LogicalThreadContext.SetData("context", c);
                // Only fetch the compositions schemas if indicated on searchDTO
                var applicationCompositionSchemata = new Dictionary<string, ApplicationCompositionSchema>();
                var hasInlineComposition = schema.Compositions.Any(comp => comp.Inline);
                if ((searchDto.CompositionsToFetch != null && searchDto.CompositionsToFetch.Count > 0) || hasInlineComposition) {
                    var allCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(schema);
                    foreach (var compositionSchema in allCompositionSchemas) {
                        if (compositionSchema.Value.INLINE || (searchDto.CompositionsToFetch != null && searchDto.CompositionsToFetch.Contains(compositionSchema.Key))) {
                            applicationCompositionSchemata.Add(compositionSchema.Key, compositionSchema.Value);
                        }
                    }
                }
                Log.DebugFormat("BaseApplicationDataSet#GetList calling Find method on maximo engine. Application Schema \"{0}\" / Context \"{1}\"", schema, c);
                entities = _maximoConnectorEngine.Find(entityMetadata, searchDto, applicationCompositionSchemata);
                // Get the composition data for the list, only in the case of detailed list (like printing details), otherwise, this is unecessary
                if (applicationCompositionSchemata.Count > 0) {
                    GetCompositionData(application, new PreFetchedCompositionFetchRequest(entities) {
                        CompositionList = new List<string>(applicationCompositionSchemata.Keys)
                    }, null);

                }
            }, ctx);

            Task.WaitAll(tasks);

            return new ApplicationListResult(totalCount, searchDto, entities, schema);
        }


        //TODO: add locale,and format options
        public IDictionary<string, BaseAssociationUpdateResult> BuildAssociationOptions(AttributeHolder dataMap, ApplicationMetadata application, IAssociationPrefetcherRequest request) {

            var associationsToFetch = AssociationHelper.BuildAssociationsToPrefetch(request, application.Schema);
            if (associationsToFetch.IsNone) {
                return new Dictionary<string, BaseAssociationUpdateResult>();
            }


            IDictionary<string, BaseAssociationUpdateResult> associationOptionsDictionary = new ConcurrentDictionary<string, BaseAssociationUpdateResult>();
            var before = LoggingUtil.StartMeasuring(Log, "starting association options fetching for application {0} schema {1}", application.Name, application.Schema.Name);

            var associations = application.Schema.Associations;
            var tasks = new List<Task>();
            var ctx = ContextLookuper.LookupContext();

            #region associations

            foreach (var applicationAssociation in associations) {
                if (!associationsToFetch.ShouldResolve(applicationAssociation.AssociationKey)) {
                    Log.Debug("ignoring association fetching: {0}".Fmt(applicationAssociation.AssociationKey));
                    continue;
                }

                //only resolve the association options for non lazy associations or lazy loaded with value set.
                SearchRequestDto search;
                if (!applicationAssociation.IsLazyLoaded()) {
                    search = new SearchRequestDto();
                } else if (dataMap != null && dataMap.GetAttribute(applicationAssociation.Target) != null) {
                    //if the field has a value, fetch only this single element, for showing eventual extra label fields... ==> lookup with a selected value
                    search = new SearchRequestDto();
                    var toAttribute = applicationAssociation.EntityAssociation.PrimaryAttribute().To;
                    var prefilledValue = dataMap.GetAttribute(applicationAssociation.Target).ToString();
                    search.AppendSearchEntry(toAttribute, prefilledValue);
                } else {
                    //lazy association with no default value
                    continue;
                }
                var association = applicationAssociation;

                tasks.Add(Task.Factory.NewThread(c => {
                    //this will avoid that one thread impacts any other, for ex: changing metadataid of the query
                    var perThreadContext = ctx.ShallowCopy();
                    Quartz.Util.LogicalThreadContext.SetData("context", perThreadContext);
                    var associationOptions = _associationOptionResolver.ResolveOptions(application, dataMap, association, search);
                    associationOptionsDictionary.Add(association.AssociationKey, new BaseAssociationUpdateResult(associationOptions));
                }, ctx));

            }
            #endregion

            #region optionfields
            foreach (var optionField in application.Schema.OptionFields) {
                if (!associationsToFetch.ShouldResolve(optionField.AssociationKey)) {
                    Log.Debug("ignoring association fetching: {0}".Fmt(optionField.AssociationKey));
                    continue;
                }

                if (optionField.ProviderAttribute == null) {
                    //if there´s no provider, there´s nothing to do --> static list
                    continue;
                }
                var field = optionField;
                tasks.Add(Task.Factory.NewThread(c => {
                    Quartz.Util.LogicalThreadContext.SetData("context", c);
                    var associationOptions = _dynamicOptionFieldResolver.ResolveOptions(application, field, dataMap);
                    if (associationOptionsDictionary.ContainsKey(field.AssociationKey)) {
                        associationOptionsDictionary.Remove(field.AssociationKey);
                    }
                    associationOptionsDictionary.Add(field.AssociationKey, new BaseAssociationUpdateResult(associationOptions));
                }, ctx));
            }
            #endregion

            Task.WaitAll(tasks.ToArray());
            if (Log.IsDebugEnabled) {
                var keys = String.Join(",", associationOptionsDictionary.Keys.Where(k => associationOptionsDictionary[k].AssociationData != null));
                Log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "Finished execution of options fetching. Resolved collections: {0}", keys));
            }


            return associationOptionsDictionary;
        }




        public virtual SynchronizationApplicationData Sync(ApplicationMetadata applicationMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData) {
            return _maximoConnectorEngine.Sync(applicationMetadata, applicationSyncData);
        }

        public MaximoResult Execute(ApplicationMetadata application, JObject json, string id, string operation) {
            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var operationWrapper = new OperationWrapper(application, entityMetadata, operation, json, id);
            return _maximoConnectorEngine.Execute(operationWrapper);
        }

        public GenericResponseResult<IDictionary<string, BaseAssociationUpdateResult>> UpdateAssociations(ApplicationMetadata application,
            AssociationUpdateRequest request, JObject currentData) {

            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var cruddata = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), entityMetadata,
                application, currentData, request.Id);
            if (EagerAssociationTrigger.Equals(request.TriggerFieldName)) {
                request.AssociationsToFetch = AssociationHelper.AllButSchema;
                return new GenericResponseResult<IDictionary<string, BaseAssociationUpdateResult>>(BuildAssociationOptions(cruddata, application, request));
            }
            return new GenericResponseResult<IDictionary<string, BaseAssociationUpdateResult>>(DoUpdateAssociation(application, request, cruddata));
        }

        protected virtual IDictionary<string, BaseAssociationUpdateResult> DoUpdateAssociation(ApplicationMetadata application, AssociationUpdateRequest request,
            AttributeHolder cruddata) {

            var before = LoggingUtil.StartMeasuring(Log, "starting update association options fetching for application {0} schema {1}", application.Name, application.Schema.Name);
            IDictionary<string, BaseAssociationUpdateResult> resultObject =
                new Dictionary<string, BaseAssociationUpdateResult>();
            ISet<string> associationsToUpdate = null;

            // Check if 'self' (for lazy load) or 'dependant' (for dependant combos) association update
            if (!String.IsNullOrWhiteSpace(request.AssociationFieldName)) {
                associationsToUpdate = new HashSet<String> { request.AssociationFieldName };
            } else if (!String.IsNullOrWhiteSpace(request.TriggerFieldName)) {
                var triggerFieldName = request.TriggerFieldName;
                if (!application.Schema.DependantFields.TryGetValue(triggerFieldName, out associationsToUpdate)) {
                    throw new InvalidOperationException();
                }
            }

            if (associationsToUpdate == null) {
                return resultObject;
            }

            var tasks = new List<Task>();
            var ctx = ContextLookuper.LookupContext();

            //there might be some associations/optionfields to be updated after the first value is selected
            foreach (var associationToUpdate in associationsToUpdate) {
                var association = application.Schema.Associations.FirstOrDefault(f => (
                    EntityUtil.IsRelationshipNameEquals(f.AssociationKey, associationToUpdate)));
                if (association == null) {
                    var optionField = application.Schema.OptionFields.First(f => f.AssociationKey == associationToUpdate);
                    tasks.Add(Task.Factory.NewThread(c => {
                        Quartz.Util.LogicalThreadContext.SetData("context", c);
                        var data = _dynamicOptionFieldResolver.ResolveOptions(application, optionField, cruddata);
                        resultObject.Add(optionField.AssociationKey, new LookupAssociationUpdateResult(data, 100, PaginatedSearchRequestDto.DefaultPaginationOptions));
                    }, ctx));
                } else {
                    var associationApplicationMetadata =
                        ApplicationAssociationResolver.GetAssociationApplicationMetadata(association);

                    var searchRequest = BuildSearchDTO(request, association, cruddata);

                    if (searchRequest == null) {
                        //this would only happen if association is lazy and there´s no default value 
                        //(cause we´d need to fetch one-value list for displaying)
                        continue;
                    }
                    var threadSafeContext = new ContextHolderWithSearchDto(ctx, searchRequest);
                    tasks.Add(Task.Factory.NewThread(c => {
                        Quartz.Util.LogicalThreadContext.SetData("context", threadSafeContext.Context);
                        var options = _associationOptionResolver.ResolveOptions(application, cruddata, association,
                           threadSafeContext.Dto);

                        resultObject.Add(association.AssociationKey,
                            new LookupAssociationUpdateResult(searchRequest.TotalCount, searchRequest.PageNumber,
                                searchRequest.PageSize, options, associationApplicationMetadata, PaginatedSearchRequestDto.DefaultPaginationOptions));
                    }, threadSafeContext));
                }
            }

            Task.WaitAll(tasks.ToArray());

            if (Log.IsDebugEnabled) {
                var keys = String.Join(",", resultObject.Keys.Where(k => resultObject[k].AssociationData != null));
                Log.Debug(LoggingUtil.BaseDurationMessageFormat(before,
                    "Finished execution of options fetching. Resolved collections: {0}", keys));
            }
            return resultObject;
        }

        class ContextHolderWithSearchDto {
            readonly ContextHolder _context;
            readonly PaginatedSearchRequestDto _dto;

            public ContextHolderWithSearchDto(ContextHolder context, PaginatedSearchRequestDto dto) {
                _context = context == null ? null : ReflectionUtil.DeepClone(context);
                _dto = dto == null ? null : ReflectionUtil.DeepClone(dto);
            }

            public ContextHolder Context {
                get {
                    return _context;
                }
            }

            public PaginatedSearchRequestDto Dto {
                get {
                    return _dto;
                }
            }

        }

        private static PaginatedSearchRequestDto BuildSearchDTO(AssociationUpdateRequest request, ApplicationAssociationDefinition association, AttributeHolder cruddata) {

            var searchRequest = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);

            if (request.SearchDTO == null) {
                request.SearchDTO = PaginatedSearchRequestDto.DefaultInstance(null);
            }
            searchRequest.PageNumber = request.SearchDTO.PageNumber;
            searchRequest.PageSize = request.SearchDTO.PageSize;
            //avoids pagination unless the association renderer defines so (lookup)
            searchRequest.ShouldPaginate = association.IsPaginated();
            searchRequest.NeedsCountUpdate = association.IsPaginated();
            var valueSearchString = request.ValueSearchString;
            if (association.IsLazyLoaded() && !request.HasClientSearch()) {
                if ((cruddata == null || cruddata.GetAttribute(association.Target) == null)) {
                    //we should not update lazy dependant associations except in one case:
                    //there´s a default value in place already for the dependent association
                    // in that case, we would need to return a 1-value list to show on screen
                    return null;
                }
                //this will force that the search would be made only on that specific value
                //ex: autocomplete server, lookups that depend upon another association
                valueSearchString = cruddata.GetAttribute(association.Target) as string;
            }

            if (request.AssociationKey != null) {
                // If association has a schema key defined, the searchDTO will be filled on client, so just copy it from request
                searchRequest.SearchParams = request.SearchDTO.SearchParams;
                searchRequest.SearchValues = request.SearchDTO.SearchValues;
            } else {
                if (!String.IsNullOrWhiteSpace(valueSearchString)) {
                    searchRequest.AppendSearchParam(association.EntityAssociation.PrimaryAttribute().To);
                    searchRequest.AppendSearchValue("%" + valueSearchString + "%");
                }
                if (!String.IsNullOrWhiteSpace(request.LabelSearchString)) {
                    AppendSearchLabelString(request, association, searchRequest);
                }
            }
            return searchRequest;
        }

        /// <summary>
        ///  this is used for both autocompleteserver or lookup to peform the search on the server based upon the labe string
        /// </summary>
        /// <param name="request"></param>
        /// <param name="association"></param>
        /// <param name="searchRequest"></param>
        private static void AppendSearchLabelString(AssociationUpdateRequest request,
            ApplicationAssociationDefinition association, PaginatedSearchRequestDto searchRequest) {
            var sbParam = new StringBuilder("(");
            var sbValue = new StringBuilder();

            foreach (var labelField in association.LabelFields) {
                sbParam.Append(labelField).Append(SearchUtils.SearchParamOrSeparator);
                sbValue.Append("%" + request.LabelSearchString + "%").Append(SearchUtils.SearchValueSeparator);
            }

            sbParam.Remove(sbParam.Length - SearchUtils.SearchParamOrSeparator.Length, SearchUtils.SearchParamOrSeparator.Length);
            sbValue.Remove(sbValue.Length - SearchUtils.SearchValueSeparator.Length, SearchUtils.SearchValueSeparator.Length);
            sbParam.Append(")");
            searchRequest.AppendSearchEntry(sbParam.ToString(), sbValue.ToString());
        }


        protected virtual void Dispose(bool disposing) {

        }

        public void Dispose() {
            Dispose(true);
        }

        internal MaximoConnectorEngine MaximoConnectorEngine {
            get {
                return _maximoConnectorEngine;
            }
        }


        public virtual string ApplicationName() {
            return null;
        }

        public virtual string ClientFilter() {
            return null;
        }

        public string SchemaId() {
            return null;
        }
    }
}
