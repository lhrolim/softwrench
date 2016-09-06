using System;
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
using softwrench.sw4.batch.api.services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API.Association.Lookup;
using softWrench.sW4.Data.API.Association.SchemaLoading;
using softWrench.sW4.Data.Filter;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Search.QuickSearch;
using softWrench.sW4.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Entities.Sliced;
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

        private IContextLookuper _contextLookuper;
        private IBatchSubmissionService _batchSubmissionService;
        private IWhereClauseFacade _whereClauseFacade;
        private FilterWhereClauseHandler _filterWhereClauseHandler;
        private QuickSearchWhereClauseHandler _quickSearchWhereClauseHandler;
        private BaseDataSetSearchHelper _baseDataSetSearchHelper;

        private AttachmentHandler _attachmentHandler;

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

        private CollectionResolver CollectionResolver {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<CollectionResolver>();
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

        protected AttachmentHandler AttachmentHandler {
            get {
                if (_attachmentHandler != null) {
                    return _attachmentHandler;
                }
                _attachmentHandler =
                    SimpleInjectorGenericFactory.Instance.GetObject<AttachmentHandler>(typeof(AttachmentHandler));
                return _attachmentHandler;
            }
        }

        protected BaseDataSetSearchHelper BaseDataSetSearchHelper {
            get {
                if (_baseDataSetSearchHelper != null) {
                    return _baseDataSetSearchHelper;
                }
                _baseDataSetSearchHelper =
                    SimpleInjectorGenericFactory.Instance.GetObject<BaseDataSetSearchHelper>(typeof(BaseDataSetSearchHelper));
                return _baseDataSetSearchHelper;
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

        protected virtual DataMap FetchDetailDataMap(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            return (DataMap)Engine().FindById(entityMetadata, request.Id, request.UserIdSitetuple);
        }

        public virtual ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var id = request.Id;
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            var applicationCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(application.Schema, user);
            DataMap dataMap;
            if (request.IsEditionRequest) {
                dataMap = FetchDetailDataMap(application, user, request);

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
            var associationResults = BuildAssociationOptions(dataMap, application.Schema, request);
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
                var preFetchedCompositionFetchRequest = new PreFetchedCompositionFetchRequest(new List<AttributeHolder> { dataMap });
                preFetchedCompositionFetchRequest.CompositionList = new List<string>(compostionsToUse.Keys);
                GetCompositionData(application, preFetchedCompositionFetchRequest, null);
            }
        }

        public virtual CompositionFetchResult GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request, JObject currentData) {
            var data = DoGetCompositionData(application, request, currentData);

            // SWWEB-2046: adding download url to the datamap
            // fetching url at drag-time does not work
            if (!data.ResultObject.ContainsKey("attachment_")) {
                return data;
            }
            HandleAttachments(data);
            return data;
        }

        protected virtual void HandleAttachments(CompositionFetchResult data) {
            var attachments = data.ResultObject["attachment_"].ResultList;
            foreach (var attachment in attachments) {
                if (!attachment.ContainsKey("docinfo_.urlname"))
                    continue;
                var docInfoURL = (string)attachment["docinfo_.urlname"];
                attachment["download_url"] = AttachmentHandler.GetFileUrl(docInfoURL);
                AttachmentHandler.BuildParsedURLName(attachment);
            }
        }

        protected virtual IDictionary<string, EntityRepository.SearchEntityResult> ResolveCompositionResult(SlicedEntityMetadata parentEntityMetadata, IDictionary<string, ApplicationCompositionSchema> compositionsToResolve, Entity parentData, PaginatedSearchRequestDto search) {
            return CollectionResolver.ResolveCollections(parentEntityMetadata, compositionsToResolve, parentData, search);
        }

        private CompositionFetchResult DoGetCompositionData(ApplicationMetadata application, CompositionFetchRequest request, JObject currentData) {
            var applicationCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(application.Schema);
            var compostionsToUse = new Dictionary<string, ApplicationCompositionSchema>();
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            IDictionary<string, EntityRepository.SearchEntityResult> result;

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
                result = CollectionResolver.ResolveCollections(entityMetadata, compostionsToUse, listOfEntities);
                return new CompositionFetchResult(result, listOfEntities.FirstOrDefault());
            }

            var cruddata = EntityBuilder.BuildFromJson<Entity>(typeof(Entity), entityMetadata, application, currentData, request.Id);

            result = ResolveCompositionResult(entityMetadata, compostionsToUse, cruddata, request.PaginatedSearch);

            return new CompositionFetchResult(result, cruddata);
        }


        public virtual ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var totalCount = searchDto.TotalCount;
            IReadOnlyList<AttributeHolder> entities = null;

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            var schema = application.Schema;
            searchDto.BuildProjection(schema);
            ContextLookuper.FillGridContext(application.Name, SecurityFacade.CurrentUser());

            if (searchDto.Key == null) {
                searchDto.Key = new ApplicationMetadataSchemaKey();
            }
            searchDto.Key.ApplicationName = application.Name;
            searchDto.Key.SchemaId = schema.SchemaId;
            searchDto.Key.Platform = ClientPlatform.Web;

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

            FilterWhereClauseHandler.HandleDTO(application.Schema, searchDto);
            QuickSearchWhereClauseHandler.HandleDTO(application.Schema, searchDto);

            var tasks = new Task[1];
            var ctx = ContextLookuper.LookupContext();

            // add pre selected filter if originally it had not a searchDTO
            // so is a menu/breadcrumb navigation
            if (searchDto.IsDefaultInstance || searchDto.AddPreSelectedFilters) {
                SchemaFilterBuilder.AddPreSelectedFilters(application.Schema.DeclaredFilters, searchDto);
            }

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
            var associationResults = BuildAssociationOptions(DataMap.BlankInstance(application.Name), application.Schema, listOptionsPrefetchRequest);
            return new ApplicationListResult(totalCount, searchDto, entities, schema, associationResults) {
                AffectedProfiles = ctx.AvailableProfilesForGrid.Select(s => s.ToDTO()),
                CurrentSelectedProfile = ctx.CurrentSelectedProfile
            };
        }




        public virtual AssociationMainSchemaLoadResult BuildAssociationOptions(AttributeHolder dataMap,
            ApplicationSchemaDefinition schema, IAssociationPrefetcherRequest request) {

            var result = new AssociationMainSchemaLoadResult();

            var associationsToFetch = AssociationHelper.BuildAssociationsToPrefetch(request, schema);
            if (associationsToFetch.IsNone) {
                return result;
            }

            IDictionary<string, IEnumerable<IAssociationOption>>
                eagerFetchedOptions = new ConcurrentDictionary<string, IEnumerable<IAssociationOption>>();

            IDictionary<string, IDictionary<string, IAssociationOption>>
                lazyOptions = new ConcurrentDictionary<string, IDictionary<string, IAssociationOption>>();

            var before = LoggingUtil.StartMeasuring(Log, AssociationLogMsg, schema.ApplicationName, schema.Name);

            var associations = schema.Associations(request.IsShowMoreMode);
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
                    var associationOptions = _associationOptionResolver.ResolveOptions(schema, dataMap, association, search);
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
            foreach (var optionField in schema.OptionFields(request.IsShowMoreMode)) {
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
                    var associationOptions = _dynamicOptionFieldResolver.ResolveOptions(schema, field, dataMap);
                    eagerFetchedOptions.Add(field.AssociationKey, associationOptions);
                }, ctx));
            }
            #endregion



            Task.WaitAll(tasks.ToArray());

            //let's handle eventual inline compositions afterwards to avoid an eventual thread explosion
            #region inlineCompositions

            if (Log.IsDebugEnabled) {
                var keys = string.Join(",", eagerFetchedOptions.Keys.Where(k => eagerFetchedOptions[k] != null)) + string.Join(",", lazyOptions.Keys.Where(k => lazyOptions[k] != null));
                Log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "Finished execution of options fetching. Resolved collections: {0}", keys));
            }

            result.EagerOptions = eagerFetchedOptions;
            result.PreFetchLazyOptions = lazyOptions;

            if (schema.HasInlineComposition && dataMap is CrudOperationData) {
                var innerCompositions = GenerateInlineCompositionResult(dataMap, schema, request);
                result.MergeWithOtherSchemas(innerCompositions);
            }

            #endregion

            return result;
        }

        private List<AssociationMainSchemaLoadResult> GenerateInlineCompositionResult(AttributeHolder dataMap, ApplicationSchemaDefinition schema,
            IAssociationPrefetcherRequest request) {

            //TODO: switch to CompositionSchemaLoadResult in order to have different eager options available
            var inlineCompositionTasks = new List<Task>();

            var result = new List<AssociationMainSchemaLoadResult>();

            var crudData = dataMap as CrudOperationData;
            var inlineCompositions = schema.Compositions().Where(c => c.Inline);
            foreach (var composition in inlineCompositions) {
                var mode = request.IsShowMoreMode
                    ? SchemaFetchMode.SecondaryContent
                    : SchemaFetchMode.MainContent;
                var listCompositionSchema = composition.Schema.Schemas.List;
                var compositionAssociations = listCompositionSchema.Associations(mode);

                if (compositionAssociations.Any()) {
                    var compositionData = (IEnumerable<CrudOperationData>)crudData.GetRelationship(composition.AssociationKey);
                    if (compositionData != null) {
                        var compositeDataMap = new CompositeDatamap(compositionData);
                        var compositionRequest = new InlineCompositionAssociationPrefetcherRequest(request, composition.AssociationKey);
                        var task = Task<AssociationMainSchemaLoadResult>.Factory.StartNew(() => BuildAssociationOptions(compositeDataMap, listCompositionSchema, compositionRequest));
                        inlineCompositionTasks.Add(task);
                    }
                }
            }

            if (!inlineCompositionTasks.Any()) {
                return result;
            }

            Task.WaitAll(inlineCompositionTasks.ToArray());
            foreach (var completedTask in inlineCompositionTasks) {
                result.Add(((Task<AssociationMainSchemaLoadResult>)completedTask).Result);
            }
            return result;
        }


        //        public virtual SynchronizationApplicationData Sync(ApplicationMetadata applicationMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData) {
        //            return Engine().Sync(applicationMetadata, applicationSyncData);
        //        }
        public virtual TargetResult Execute(ApplicationMetadata application, JObject json, OperationDataRequest operationData) {
            var compositionData = operationData.CompositionData;
            if (compositionData == null || !compositionData.Operation.EqualsAny(OperationConstants.CRUD_DELETE, OperationConstants.CRUD_UPDATE)) {
                //not a composition deletion/update, no need for any further checking
                return Execute(application, json, operationData.Id, operationData.Operation, operationData.Batch,
                    new Tuple<string, string>(operationData.UserId, operationData.SiteId));
            }

            var clientComposition = compositionData.DispatcherComposition;
            var composition = application.Schema.Compositions().FirstOrDefault(f => f.Relationship.Equals(EntityUtil.GetRelationshipName(clientComposition)));
            if (composition == null) {
                return Execute(application, json, operationData.Id, operationData.Operation, operationData.Batch,
                  new Tuple<string, string>(operationData.UserId, operationData.SiteId));
            }
            var compositionListSchema = composition.Schema.Schemas.List;
            var compositionEntityName = compositionListSchema.EntityName;
            var compositionEntity = MetadataProvider.Entity(compositionEntityName);
            var maximoWebServiceName = compositionEntity.ConnectorParameters.GetWSEntityKey();
            if (maximoWebServiceName == null) {
                //let parent web-service handle it
                return Execute(application, json, operationData.Id, operationData.Operation, operationData.Batch,
                    new Tuple<string, string>(operationData.UserId, operationData.SiteId));
            }

            if (compositionEntity.ConnectorParameters.Parameters.ContainsKey("integration_interface_operations")) {
                var validOperations = compositionEntity.ConnectorParameters.Parameters["integration_interface_operations"].Split(',');
                if (!validOperations.Any(a => a.EqualsIc(compositionData.Operation))) {
                    //not to be handled by composed web-service either
                    return Execute(application, json, operationData.Id, operationData.Operation, operationData.Batch,
                        new Tuple<string, string>(operationData.UserId, operationData.SiteId));
                }

            }

            //if there´s an specific web-service for the child entity, let´s use it
            var compositionSchemaToUse = compositionData.Operation.EqualsIc(OperationConstants.CRUD_DELETE)
                ? composition.Schema.Schemas.List
                : composition.Schema.Schemas.Detail;

            var compositionApplication = ApplicationMetadata.FromSchema(compositionSchemaToUse);

            var ds = DataSetProvider.GetInstance().LookupDataSet(compositionApplication.Name, compositionSchemaToUse.SchemaId);
            if (ds.GetType() != typeof(BaseApplicationDataSet)) {
                //if there´s an overriden DataSet for the composition, let´s use it
                var targetResult = ds.Execute(compositionApplication, GetCompositionJson(json, compositionData), compositionData.Id, compositionData.Operation,
                    operationData.Batch, new Tuple<string, string>(operationData.UserId, operationData.SiteId));

                //let's make sure the success message receives the right userId, which is the parent userid
                targetResult.UserId = operationData.UserId;

                return targetResult;
            }
            //otherwise let´s stick with the main app dataset
            return Execute(compositionApplication, GetCompositionJson(json, compositionData), compositionData.Id, compositionData.Operation,
                operationData.Batch,
                new Tuple<string, string>(operationData.UserId, operationData.SiteId));

        }

        private JObject GetCompositionJson(JObject json, CompositionOperationDTO compositionOperationDTO) {
            if (compositionOperationDTO.CompositionItem != null) {
                return compositionOperationDTO.CompositionItem;
            }
            var relationship = compositionOperationDTO.DispatcherComposition;
            var val = json.GetValue(relationship) as JContainer;
            if (val == null) {
                return json;
            }
            return val[0] as JObject;
        }

        public virtual TargetResult Execute(ApplicationMetadata application, JObject json, string id, string operation, Boolean isBatch, Tuple<string, string> userIdSite) {
            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var operationWrapper = new OperationWrapper(application, entityMetadata, operation, json, id);
            if (userIdSite != null) {
                operationWrapper.UserId = userIdSite.Item1;
                operationWrapper.SiteId = userIdSite.Item2;
            }

            if (isBatch) {
                return BatchSubmissionService.CreateAndSubmit(operationWrapper.ApplicationMetadata.Name, operationWrapper.ApplicationMetadata.Schema.SchemaId, operationWrapper.JSON);
            }

            return DoExecute(operationWrapper);
        }

        public virtual TargetResult DoExecute(OperationWrapper operationWrapper) {
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
            var associationApplicationMetadata = ApplicationAssociationResolver.GetAssociationApplicationMetadata(association);

            if (associationApplicationMetadata != null
                && associationApplicationMetadata.Schema != null
                && associationApplicationMetadata.Schema.SchemaFilters != null
                && lookupRequest.SearchDTO.AddPreSelectedFilters) {
                SchemaFilterBuilder.AddPreSelectedFilters(associationApplicationMetadata.Schema.SchemaFilters, lookupRequest.SearchDTO);
            }

            var options = _associationOptionResolver.ResolveOptions(application.Schema, cruddata, association, lookupRequest.SearchDTO);

            if (Log.IsDebugEnabled) {
                Log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "Finished execution of options fetching. Resolved collections: {0}"));
            }

            return new LookupOptionsFetchResultDTO(lookupRequest.SearchDTO.TotalCount, lookupRequest.SearchDTO.PageNumber, lookupRequest.SearchDTO.PageSize, options, associationApplicationMetadata, lookupRequest.SearchDTO);
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
                        var data = _dynamicOptionFieldResolver.ResolveOptions(application.Schema, optionField, cruddata);
                        if (data != null) {
                            resultObject.Add(optionField.AssociationKey, new LookupOptionsFetchResultDTO(data, 100, PaginatedSearchRequestDto.DefaultPaginationOptions));
                        }
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

                        var options = _associationOptionResolver.ResolveOptions(application.Schema, cruddata, association,
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

        public IEnumerable<IAssociationOption> GetSWPriorityType(OptionFieldProviderParameters parameters) {
            //create default priority list
            var list = new List<AssociationOption>();

            //list.Add(new AssociationOption("0", "0 - None"));
            list.Add(new AssociationOption("1", "1 - High"));
            list.Add(new AssociationOption("2", "2 - Medium"));
            list.Add(new AssociationOption("3", "3 - Low"));

            return list;
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
