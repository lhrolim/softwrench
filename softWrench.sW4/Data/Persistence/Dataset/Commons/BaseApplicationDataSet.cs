﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.Util;
using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.services;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.Collection;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Security.Context;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using cts.commons.simpleinjector;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API.Association.Lookup;
using softWrench.sW4.Data.API.Association.SchemaLoading;
using softWrench.sW4.Data.Filter;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using EntityUtil = softWrench.sW4.Util.EntityUtil;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {

    /// <summary>
    /// 
    /// This class defines the base contract for all persistence operations via the metadata-portion of the framework, 
    /// regardless if on maximo or swdb engines.
    /// 
    /// <remarks>
    /// This class should not be inherited by clients
    /// 
    /// Use either <see cref="MaximoApplicationDataSet"/> or <see cref="SWDBApplicationDataset"/>
    /// </remarks>
    /// 
    /// <seealso cref="MaximoApplicationDataSet"/>
    /// <seealso cref="SWDBApplicationDataset"/>
    /// 
    /// </summary>
    public abstract class BaseApplicationDataSet : IDataSet {

        /// <summary>
        /// if this constant is present we´re interested in getting all the eagerassociations of the schema, rather then a specific association dependency
        /// </summary>
        private const string EagerAssociationTrigger = "#eagerassociations";

        private const string AssociationLogMsg = "starting association options fetching for application {0} schema {1}";

        protected static readonly ILog Log = LogManager.GetLogger(typeof(BaseApplicationDataSet));

        private readonly ApplicationAssociationResolver _associationOptionResolver = new ApplicationAssociationResolver();
        private readonly DynamicOptionFieldResolver _dynamicOptionFieldResolver = new DynamicOptionFieldResolver();
        private readonly CollectionResolver _collectionResolver = new CollectionResolver();

        private IContextLookuper _contextLookuper;
        private IBatchSubmissionService _batchSubmissionService;
        private IWhereClauseFacade _whereClauseFacade;
        private FilterWhereClauseHandler _filterWhereClauseHandler;
        private QuickSearchWhereClauseHandler _quickSearchWhereClauseHandler;

        internal BaseApplicationDataSet() {
        }

        protected IContextLookuper ContextLookuper {
            get {
                if (_contextLookuper != null) {
                    return _contextLookuper;
                }
                _contextLookuper = SimpleInjectorGenericFactory.Instance.GetObject<IContextLookuper>(typeof(IContextLookuper));
                return _contextLookuper;
            }
        }

        protected FilterWhereClauseHandler FilterWhereClauseHandler {
            get {
                if (_filterWhereClauseHandler != null) {
                    return _filterWhereClauseHandler;
                }
                _filterWhereClauseHandler =
                    SimpleInjectorGenericFactory.Instance.GetObject<FilterWhereClauseHandler>(typeof(FilterWhereClauseHandler));
                return _filterWhereClauseHandler;
            }
        }

        protected IBatchSubmissionService BatchSubmissionService {
            get {
                if (_batchSubmissionService != null) {
                    return _batchSubmissionService;
                }
                _batchSubmissionService =
                    SimpleInjectorGenericFactory.Instance.GetObject<IBatchSubmissionService>(typeof(IBatchSubmissionService));
                return _batchSubmissionService;
            }
        }

        protected IWhereClauseFacade WhereClauseFacade {
            get {
                if (_whereClauseFacade != null) {
                    return _whereClauseFacade;
                }
                _whereClauseFacade =
                    SimpleInjectorGenericFactory.Instance.GetObject<IWhereClauseFacade>(typeof(IWhereClauseFacade));
                return _whereClauseFacade;
            }
        }

        protected QuickSearchWhereClauseHandler QuickSearchWhereClauseHandler {
            get {
                if (_quickSearchWhereClauseHandler != null) {
                    return _quickSearchWhereClauseHandler;
                }
                _quickSearchWhereClauseHandler =
                    SimpleInjectorGenericFactory.Instance.GetObject<QuickSearchWhereClauseHandler>(typeof(QuickSearchWhereClauseHandler));
                return _quickSearchWhereClauseHandler;
            }
        }

        protected abstract IConnectorEngine Engine();


        public Int32 GetCount(ApplicationMetadata application, [CanBeNull]IDataRequest request) {


            SearchRequestDto searchDto = null;
            if (request is DataRequestAdapter) {
                searchDto = ((DataRequestAdapter)request).SearchDTO;
            } else if (request is SearchRequestDto) {
                searchDto = (SearchRequestDto)request;
            }
            searchDto = searchDto ?? new SearchRequestDto();

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            searchDto.BuildProjection(application.Schema);

            return Engine().Count(entityMetadata, searchDto);
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
                return GetApplicationDetail(application, user, (DetailRequest)request);
            }
            if (application.Schema.Stereotype == SchemaStereotype.List) {
                return GetList(application, PaginatedSearchRequestDto.DefaultInstance(application.Schema));
            }
            if (application.Schema.Stereotype == SchemaStereotype.Detail || application.Schema.Stereotype == SchemaStereotype.DetailNew || request.Key.Mode == SchemaMode.input) {
                //case of detail of new items
                return GetApplicationDetail(application, user, DetailRequest.GetInstance(application, adapter));
            }
            throw new InvalidOperationException("could not determine which operation to take upon request");
        }

        public virtual ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var id = request.Id;
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            var applicationCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(application.Schema);
            DataMap dataMap;
            if (request.IsEditionRequest) {
                dataMap = (DataMap)Engine().FindById(entityMetadata, id, request.UserIdSitetuple);

                if (dataMap == null) {
                    return null;
                }
                LoadCompositions(application, request, applicationCompositionSchemas, dataMap);

                if (request.InitialValues != null) {
                    var initValDataMap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application,
                        request.InitialValues, entityMetadata.Schema.MappingType);
                    dataMap = DefaultValuesBuilder.AddMissingInitialValues(dataMap, initValDataMap);
                }
            } else {
                //creation of items
                dataMap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application, request.InitialValues, entityMetadata.Schema.MappingType);
            }
            var associationResults = BuildAssociationOptions(dataMap, application, request);
            var detailResult = new ApplicationDetailResult(dataMap, associationResults, application.Schema, applicationCompositionSchemas, id);
            return detailResult;
        }

        private void LoadCompositions(ApplicationMetadata application, DetailRequest request,
            IDictionary<string, ApplicationCompositionSchema> applicationCompositionSchemas, DataMap dataMap) {
            var prefetchCompositions =
                "true".EqualsIc(application.Schema.GetProperty(ApplicationSchemaPropertiesCatalog.PreFetchCompositions)) ||
                "#all".Equals(request.CompositionsToFetch);
            var compositionList = request.CompositionsToFetch == null
                ? new List<string>()
                : new List<string>(request.CompositionsToFetch.Split(','));
            var compostionsToUse = new Dictionary<string, ApplicationCompositionSchema>();
            foreach (var compositionEntry in applicationCompositionSchemas) {
                if (prefetchCompositions || FetchType.Eager.Equals(compositionEntry.Value.FetchType) ||
                    compositionEntry.Value.INLINE || compositionList.Contains(compositionEntry.Key)) {
                    compostionsToUse.Add(compositionEntry.Key, compositionEntry.Value);
                }
            }
            if (compostionsToUse.Any()) {
                GetCompositionData(application, new PreFetchedCompositionFetchRequest(new List<AttributeHolder> { dataMap }), null);
            }
        }

        public virtual CompositionFetchResult GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request, JObject currentData) {

            var applicationCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(application.Schema);
            var compostionsToUse = new Dictionary<string, ApplicationCompositionSchema>();
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            Dictionary<string, EntityRepository.SearchEntityResult> result;

            if (request.CompositionList != null) {
                foreach (var compositionKey in request.CompositionList.Where(applicationCompositionSchemas.ContainsKey)) {
                    //use only the passed compositions amongst the original list
                    compostionsToUse.Add(compositionKey, applicationCompositionSchemas[compositionKey]);
                }
            } else {
                //use them all
                compostionsToUse = (Dictionary<string, ApplicationCompositionSchema>)applicationCompositionSchemas;
            }

            if (request is PreFetchedCompositionFetchRequest) {
                var listOfEntities = ((PreFetchedCompositionFetchRequest)request).PrefetchEntities;
                result = _collectionResolver.ResolveCollections(entityMetadata, compostionsToUse, listOfEntities);
                return new CompositionFetchResult(result, listOfEntities.FirstOrDefault());
            }


            var cruddata = EntityBuilder.BuildFromJson<Entity>(typeof(Entity), entityMetadata,
               application, currentData, request.Id);

            result = _collectionResolver.ResolveCollections(entityMetadata, compostionsToUse, cruddata, request.PaginatedSearch);

            return new CompositionFetchResult(result, cruddata);
        }


        public virtual ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var totalCount = searchDto.TotalCount;
            IReadOnlyList<AttributeHolder> entities = null;

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            var schema = application.Schema;
            searchDto.BuildProjection(schema);
            ContextLookuper.FillGridContext(application.Name, SecurityFacade.CurrentUser());

            if (searchDto.Context != null && searchDto.Context.MetadataId != null) {
                searchDto.QueryAlias = searchDto.Context.MetadataId;
            } else {
                searchDto.QueryAlias = application.Name + "." + schema.SchemaId;
            }

            var propertyValue = schema.GetProperty(ApplicationSchemaPropertiesCatalog.ListSchemaOrderBy);
            if (searchDto.SearchSort == null && propertyValue != null) {
                //if the schema has a default sort defined, and we didn´t especifally asked for any sort column, apply the default schema
                searchDto.SearchSort = propertyValue;
            }

            searchDto = string.IsNullOrEmpty(searchDto.QuickSearchData)
                ? FilterWhereClauseHandler.HandleDTO(application.Schema, searchDto)
                : QuickSearchWhereClauseHandler.HandleDTO(application.Schema, searchDto);

            var tasks = new Task[1];
            var ctx = ContextLookuper.LookupContext();

            //count query
            tasks[0] = Task.Factory.NewThread(c => {
                Quartz.Util.LogicalThreadContext.SetData("context", c);
                if (searchDto.NeedsCountUpdate) {
                    Log.DebugFormat("BaseApplicationDataSet#GetList calling Count method on maximo engine. Application Schema \"{0}\" / Context \"{1}\"", schema, c);
                    totalCount = Engine().Count(entityMetadata, searchDto.ShallowCopy());
                }
            }, ctx);

            //query

            var dto = (PaginatedSearchRequestDto)searchDto.ShallowCopy();
            Quartz.Util.LogicalThreadContext.SetData("context", ctx);
            // Only fetch the compositions schemas if indicated on searchDTO
            var applicationCompositionSchemata = new Dictionary<string, ApplicationCompositionSchema>();
            if (searchDto.CompositionsToFetch != null && searchDto.CompositionsToFetch.Count > 0) {
                var allCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(schema);
                foreach (var compositionSchema in allCompositionSchemas) {
                    if (searchDto.CompositionsToFetch.Contains(compositionSchema.Key)) {
                        applicationCompositionSchemata.Add(compositionSchema.Key, compositionSchema.Value);
                    }
                }
            }
            if (schema.Properties.ContainsKey(ApplicationSchemaPropertiesCatalog.DisablePagination)) {
                dto.ShouldPaginate = dto.ShouldPaginate && !"true".Equals(schema.Properties[ApplicationSchemaPropertiesCatalog.DisablePagination]);
            }
            Log.DebugFormat("BaseApplicationDataSet#GetList calling Find method on maximo engine. Application Schema \"{0}\" / Context \"{1}\"", schema, ctx);
            entities = Engine().Find(entityMetadata, dto, applicationCompositionSchemata);

            // Get the composition data for the list, only in the case of detailed list (like printing details), otherwise, this is unecessary
            if (applicationCompositionSchemata.Count > 0) {
                var request = new PreFetchedCompositionFetchRequest(entities) {
                    CompositionList = new List<string>(applicationCompositionSchemata.Keys)
                };
                GetCompositionData(application, request, null);
            }



            Task.WaitAll(tasks);
            var listOptionsPrefetchRequest = new ListOptionsPrefetchRequest();
            var associationResults = BuildAssociationOptions(DataMap.BlankInstance(application.Name), application, listOptionsPrefetchRequest);
            return new ApplicationListResult(totalCount, searchDto, entities, schema, associationResults) {
                AffectedProfiles = ctx.AvailableProfilesForGrid.Select(s => s.ToDTO()),
                CurrentSelectedProfile = ctx.CurrentSelectedProfile
            };
        }


        public AssociationMainSchemaLoadResult BuildAssociationOptions(AttributeHolder dataMap,
            ApplicationMetadata application, IAssociationPrefetcherRequest request) {

            var result = new AssociationMainSchemaLoadResult();

            var associationsToFetch = AssociationHelper.BuildAssociationsToPrefetch(request, application.Schema);
            if (associationsToFetch.IsNone) {
                return result;
            }


            IDictionary<string, IEnumerable<IAssociationOption>>
                eagerFetchedOptions = new ConcurrentDictionary<string, IEnumerable<IAssociationOption>>();

            IDictionary<string, IDictionary<string, IAssociationOption>>
                lazyOptions = new ConcurrentDictionary<string, IDictionary<string, IAssociationOption>>();

            var before = LoggingUtil.StartMeasuring(Log, AssociationLogMsg, application.Name, application.Schema.Name);

            var associations = application.Schema.Associations(request.IsShowMoreMode);
            var tasks = new List<Task>();
            var ctx = ContextLookuper.LookupContext();

            #region associations

            System.Collections.Generic.ISet<string> handledAssociations = new HashSet<string>();

            foreach (var applicationAssociation in associations) {
                if (!associationsToFetch.ShouldResolve(applicationAssociation.AssociationKey)) {
                    Log.Debug("ignoring association fetching: {0}".Fmt(applicationAssociation.AssociationKey));
                    continue;
                }
                if (handledAssociations.Contains(applicationAssociation.AssociationKey)) {
                    //this happens if a schema has multiple associations defined like sections that share the same fields, 
                    //but that are never visible at the same time
                    continue;
                }

                handledAssociations.Add(applicationAssociation.AssociationKey);

                //only resolve the association options for non lazy associations or (lazy loaded with value set or reverse associations)
                var search = AssociationHelper.BuildAssociationFilter(dataMap, applicationAssociation);
                if (search == null) {
                    continue;
                }
                var association = applicationAssociation;

                tasks.Add(Task.Factory.NewThread(c => {
                    Quartz.Util.LogicalThreadContext.SetData("context", c);
                    var associationOptions = _associationOptionResolver.ResolveOptions(application, dataMap, association, search);
                    var associationData = (associationOptions == null) ? new IAssociationOption[0] : associationOptions.ToArray();
                    if (association.IsEagerLoaded()) {
                        eagerFetchedOptions.Add(association.AssociationKey, associationData);
                    } else {
                        if (!lazyOptions.ContainsKey(association.AssociationKey)) {
                            lazyOptions[association.AssociationKey] = new Dictionary<string, IAssociationOption>();
                        }
                        if (associationData.Any()) {
                            foreach (var associationOption in associationData) {
                                //usually we´ll have just one option, unless we´re dealing with associations of inline compositions
                                lazyOptions[association.AssociationKey].Add(associationOption.Value.ToLower(), associationOption);
                            }

                        }


                    }
                }, ctx));

            }
            #endregion

            #region optionfields
            foreach (var optionField in application.Schema.OptionFields(request.IsShowMoreMode)) {
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
                    eagerFetchedOptions.Add(field.AssociationKey, associationOptions);
                }, ctx));
            }
            #endregion



            Task.WaitAll(tasks.ToArray());

            //let's handle eventual inline compositions afterwards to avoid an eventual thread explosion
            #region inlineCompositions
            if (application.Schema.HasInlineComposition && dataMap is CrudOperationData) {
                var crudData = dataMap as CrudOperationData;
                var inlineCompositions = application.Schema.Compositions().Where(c => c.Inline);
                foreach (var composition in inlineCompositions) {

                    SchemaFetchMode mode = request.IsShowMoreMode
                        ? SchemaFetchMode.SecondaryContent
                        : SchemaFetchMode.MainContent;
                    var compositionAssociations = composition.Schema.Schemas.List.Associations(mode);
                    if (compositionAssociations.Any()){
                        var compositionData = (IEnumerable<CrudOperationData>)crudData.GetRelationship(composition.AssociationKey);
                        if (compositionData != null) {
                            var compositeData = dataMap.GetAttribute(composition.AssociationKey);
                        }
                    }

                    
                }
            }
            #endregion


            if (Log.IsDebugEnabled) {
                var keys = string.Join(",", eagerFetchedOptions.Keys.Where(k => eagerFetchedOptions[k] != null)) + string.Join(",", lazyOptions.Keys.Where(k => lazyOptions[k] != null));
                Log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "Finished execution of options fetching. Resolved collections: {0}", keys));
            }

            result.EagerOptions = eagerFetchedOptions;
            result.PreFetchLazyOptions = lazyOptions;



            return result;
        }




        //        public virtual SynchronizationApplicationData Sync(ApplicationMetadata applicationMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData) {
        //            return Engine().Sync(applicationMetadata, applicationSyncData);
        //        }

        public virtual TargetResult Execute(ApplicationMetadata application, JObject json, string id, string operation, Boolean isBatch) {
            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var operationWrapper = new OperationWrapper(application, entityMetadata, operation, json, id);
            if (isBatch) {
                return BatchSubmissionService.CreateAndSubmit(operationWrapper.ApplicationMetadata.Name, operationWrapper.ApplicationMetadata.Schema.SchemaId, operationWrapper.JSON);
            }

            return Engine().Execute(operationWrapper);
        }

        public GenericResponseResult<IDictionary<string, BaseAssociationUpdateResult>> UpdateAssociations(ApplicationMetadata application,
            AssociationUpdateRequest request, JObject currentData) {

            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var cruddata = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), entityMetadata,
                application, currentData, request.Id);
            //            if (EagerAssociationTrigger.Equals(request.TriggerFieldName)) {
            //                request.AssociationsToFetch = AssociationHelper.AllButSchema;
            //                return new GenericResponseResult<IDictionary<string, BaseAssociationUpdateResult>>(BuildAssociationOptions(cruddata, application, request));
            //            }
            return new GenericResponseResult<IDictionary<string, BaseAssociationUpdateResult>>(DoUpdateAssociation(application, request, cruddata));
        }


        public virtual LookupOptionsFetchResultDTO GetLookupOptions(ApplicationMetadata application, LookupOptionsFetchRequestDTO lookupRequest, AttributeHolder cruddata) {
            var before = LoggingUtil.StartMeasuring(Log, "fetching lookup options for application {0} schema {1}", application.Name, application.Schema.Name);

            var association = application.Schema.Associations().FirstOrDefault(f => (EntityUtil.IsRelationshipNameEquals(f.AssociationKey, lookupRequest.AssociationFieldName)));

            var options = _associationOptionResolver.ResolveOptions(application, cruddata, association, lookupRequest.SearchDTO); ;

            if (Log.IsDebugEnabled) {
                Log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "Finished execution of options fetching. Resolved collections: {0}"));
            }

            var associationApplicationMetadata =
                        ApplicationAssociationResolver.GetAssociationApplicationMetadata(association);

            return new LookupOptionsFetchResultDTO(lookupRequest.SearchDTO.TotalCount, lookupRequest.SearchDTO.PageNumber, lookupRequest.SearchDTO.PageSize, options, associationApplicationMetadata);
        }

        /// <summary>
        ///  This will get used by lookup and autocomplete searches, as well as dependat associations changes (the original purpose) and possibly in other scenarios that needs documentation
        /// </summary>
        /// <param name="application"></param>
        /// <param name="request"></param>
        /// <param name="cruddata"></param>
        /// <returns></returns>
            //TODO: refactor this code, removing lookup and autocomplete searches to a specific method
        protected virtual IDictionary<string, BaseAssociationUpdateResult> DoUpdateAssociation(ApplicationMetadata application, AssociationUpdateRequest request,
            AttributeHolder cruddata) {

            var before = LoggingUtil.StartMeasuring(Log, "starting update association options fetching for application {0} schema {1}", application.Name, application.Schema.Name);
            IDictionary<string, BaseAssociationUpdateResult> resultObject =
                new Dictionary<string, BaseAssociationUpdateResult>();
            System.Collections.Generic.ISet<string> associationsToUpdate = null;

            // Check if 'self' (for lazy load) or 'dependant' (for dependant combos) association update
            if (!String.IsNullOrWhiteSpace(request.AssociationFieldName)) {
                associationsToUpdate = new HashSet<String> { request.AssociationFieldName };
            } else if (!String.IsNullOrWhiteSpace(request.TriggerFieldName)) {
                var triggerFieldName = request.TriggerFieldName;
                if (!application.Schema.DependantFields().TryGetValue(triggerFieldName, out associationsToUpdate)) {
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
                var association = application.Schema.Associations().FirstOrDefault(f => (
                    EntityUtil.IsRelationshipNameEquals(f.AssociationKey, associationToUpdate)));
                if (association == null) {
                    var optionField = application.Schema.OptionFields().First(f => f.AssociationKey.EqualsIc(associationToUpdate));
                    tasks.Add(Task.Factory.NewThread(c => {
                        Quartz.Util.LogicalThreadContext.SetData("context", c);
                        var data = _dynamicOptionFieldResolver.ResolveOptions(application, optionField, cruddata);
                        resultObject.Add(optionField.AssociationKey, new LookupOptionsFetchResultDTO(data, 100, PaginatedSearchRequestDto.DefaultPaginationOptions));
                    }, ctx));
                } else {
                    var associationApplicationMetadata =
                        ApplicationAssociationResolver.GetAssociationApplicationMetadata(association);

                    var searchRequest = BaseDataSetSearchHelper.BuildSearchDTOForAssociationSearch(request, association, cruddata);

                    if (searchRequest == null) {
                        //this would only happen if association is lazy and there´s no default value 
                        //(cause we´d need to fetch one-value list for displaying)
                        continue;
                    }

                    tasks.Add(Task.Factory.NewThread(c => {
                        var perThreadContext = ctx.ShallowCopy();
                        Quartz.Util.LogicalThreadContext.SetData("context", perThreadContext);

                        var options = _associationOptionResolver.ResolveOptions(application, cruddata, association,
                            searchRequest);

                        resultObject.Add(association.AssociationKey,
                            new LookupOptionsFetchResultDTO(searchRequest.TotalCount, searchRequest.PageNumber,
                                searchRequest.PageSize, options, associationApplicationMetadata));
                    }, ctx));
                }
            }

            Task.WaitAll(tasks.ToArray());

            if (Log.IsDebugEnabled) {
                var keys = string.Join(",", resultObject.Keys.Where(k => resultObject[k].AssociationData != null));
                Log.Debug(LoggingUtil.BaseDurationMessageFormat(before,
                    "Finished execution of options fetching. Resolved collections: {0}", keys));
            }
            return resultObject;
        }


        public virtual string SchemaId() {
            return null;
        }


        public virtual string ApplicationName() {
            return null;
        }

        public virtual string ClientFilter() {
            return null;
        }
    }
}
