using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
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
using softWrench.sW4.Metadata.Entities;
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

        internal BaseApplicationDataSet() {
        }

        protected IContextLookuper ContextLookuper => SimpleInjectorGenericFactory.Instance.GetObject<IContextLookuper>(typeof(IContextLookuper));

        [Import]
        public FilterDTOHandlerComposite FilterDTOHandlerComposite { get; set; }

        [Import]
        public CollectionResolver CollectionResolver {
            get; set;
        }

   

        //TODO: fix BatchReportEmailService which breaks on unit tests, in case of [Import] usage
        public IBatchSubmissionService BatchSubmissionService => SimpleInjectorGenericFactory.Instance.GetObject<IBatchSubmissionService>();

        [Import]
        public IWhereClauseFacade WhereClauseFacade {
            get; set;
        }


        public AttachmentHandler AttachmentHandler => SimpleInjectorGenericFactory.Instance.GetObject<AttachmentHandler>();

        [Import]
        public BaseDataSetSearchHelper BaseDataSetSearchHelper {
            get; set;
        }

        protected abstract IConnectorEngine Engine();


        public async Task<int> GetCount(ApplicationMetadata application, [CanBeNull]IDataRequest request) {


            SearchRequestDto searchDto = null;
            if (request is DataRequestAdapter) {
                searchDto = ((DataRequestAdapter)request).SearchDTO;
            } else if (request is SearchRequestDto) {
                searchDto = (SearchRequestDto)request;
            }
            searchDto = searchDto ?? new SearchRequestDto();

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            searchDto.BuildProjection(application.Schema);

            return await Engine().Count(entityMetadata, searchDto);
        }

        public async Task<IApplicationResponse> Get(ApplicationMetadata application, InMemoryUser user, IDataRequest request) {
            var adapter = request as DataRequestAdapter;
            if (adapter != null) {
                if (adapter.Id != null) {
                    request = DetailRequest.GetInstance(application, adapter);
                } else if (adapter.SearchDTO != null) {
                    request = adapter.SearchDTO;
                }
            }

            if (request is PaginatedSearchRequestDto) {
                return await GetList(application, (PaginatedSearchRequestDto)request);
            }
            if (request is DetailRequest) {
                return await GetApplicationDetail(application, user, (DetailRequest)request);
            }
            if (application.Schema.Stereotype == SchemaStereotype.List) {
                return await GetList(application, PaginatedSearchRequestDto.DefaultInstance(application.Schema));
            }
            if (application.Schema.Stereotype == SchemaStereotype.Detail || application.Schema.Stereotype == SchemaStereotype.DetailNew || request.Key.Mode == SchemaMode.input) {
                //case of detail of new items
                return await GetApplicationDetail(application, user, DetailRequest.GetInstance(application, adapter));
            }
            throw new InvalidOperationException("could not determine which operation to take upon request");
        }

        /// <summary>
        /// Convenience map for extending only the main datamap loading, instead of the whole composition logic
        /// </summary>
        /// <param name="application"></param>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual async Task<DataMap> FetchDetailDataMap(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            return (DataMap)await Engine().FindById(entityMetadata, request.Id, request.UserIdSitetuple);
        }

        public virtual async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var id = request.Id;
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            var applicationCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(application.Schema, user);
            DataMap dataMap;
            if (request.IsEditionRequest) {
                dataMap = await FetchDetailDataMap(application, user, request);

                if (dataMap == null) {
                    return null;
                }
                await LoadCompositions(application, request, applicationCompositionSchemas, dataMap);

                if (request.InitialValues != null) {
                    var initValDataMap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application,
                        request.InitialValues, entityMetadata.Schema.MappingType);
                    dataMap = DefaultValuesBuilder.AddMissingInitialValues(dataMap, initValDataMap);
                }
            } else {
                //creation of items
                dataMap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application, request.InitialValues, entityMetadata.Schema.MappingType);
            }
            var associationResults = await BuildAssociationOptions(dataMap, application.Schema, request);
            var detailResult = new ApplicationDetailResult(dataMap, associationResults, application.Schema, applicationCompositionSchemas, id);
            return detailResult;
        }

        private async Task LoadCompositions(ApplicationMetadata application, DetailRequest request,
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
                await GetCompositionData(application, preFetchedCompositionFetchRequest, null);
            }
        }

        public virtual async Task<CompositionFetchResult> GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request, JObject currentData) {
            var data = await DoGetCompositionData(application, request, currentData);

            // SWWEB-2046: adding download url to the datamap
            // fetching url at drag-time does not work
            if (!data.ResultObject.ContainsKey("attachment_")) {
                return data;
            }
            await HandleAttachments(data);
            return data;
        }

        protected virtual async Task HandleAttachments(CompositionFetchResult data) {
            var attachments = data.ResultObject["attachment_"].ResultList;
            foreach (var attachment in attachments) {
                if (!attachment.ContainsKey("docinfo_.urlname"))
                    continue;
                var docInfoURL = (string)attachment["docinfo_.urlname"];
                attachment["download_url"] = await AttachmentHandler.GetFileUrl(docInfoURL);
                AttachmentHandler.BuildParsedURLName(attachment);
            }
        }

        protected virtual async Task<IDictionary<string, EntityRepository.SearchEntityResult>> ResolveCompositionResult(SlicedEntityMetadata parentEntityMetadata, IDictionary<string, ApplicationCompositionSchema> compositionsToResolve, Entity parentData, PaginatedSearchRequestDto search) {
            return await CollectionResolver.ResolveCollections(parentEntityMetadata, compositionsToResolve, parentData, search);
        }

        private async Task<CompositionFetchResult> DoGetCompositionData(ApplicationMetadata application, CompositionFetchRequest request, JObject currentData) {
            var applicationCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(application.Schema, SecurityFacade.CurrentUser());
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
                result = await CollectionResolver.ResolveCollections(entityMetadata, compostionsToUse, listOfEntities);
                return new CompositionFetchResult(result, listOfEntities.FirstOrDefault());
            }

            var cruddata = EntityBuilder.BuildFromJson<Entity>(typeof(Entity), entityMetadata, application, currentData, request.Id);

            result = await ResolveCompositionResult(entityMetadata, compostionsToUse, cruddata, request.PaginatedSearch);

            return new CompositionFetchResult(result, cruddata);
        }


        public virtual async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
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
            FilterDTOHandlerComposite.HandleDTO(application.Schema, searchDto);

            var ctx = ContextLookuper.LookupContext();

            // add pre selected filter if originally it had not a searchDTO so is a menu/breadcrumb navigation
            if (searchDto.IsDefaultInstance || searchDto.AddPreSelectedFilters) {
                SchemaFilterBuilder.AddPreSelectedFilters(application.Schema.DeclaredFilters, searchDto);
            }

            // no sort or multisort - adds the default sort
            if ((searchDto.MultiSearchSort == null || searchDto.MultiSearchSort.Count == 0) && string.IsNullOrEmpty(searchDto.SearchSort)) {
                SearchUtils.AddDefaultSort(application.Schema, searchDto);
            }

            //count query
            if (searchDto.NeedsCountUpdate) {
                // Quartz.Util.LogicalThreadContext.SetData("context", ctx);
                Log.DebugFormat("BaseApplicationDataSet#GetList calling Count method on maximo engine. Application Schema \"{0}\" / Context \"{1}\"", schema, ctx);
                totalCount = await Engine().Count(entityMetadata, searchDto.ShallowCopy());
            }

            //query

            var dto = (PaginatedSearchRequestDto)searchDto.ShallowCopy();
            // Quartz.Util.LogicalThreadContext.SetData("context", ctx);
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
            entities = await Engine().Find(entityMetadata, dto, applicationCompositionSchemata);


            // Get the composition data for the list, only in the case of detailed list (like printing details), otherwise, this is unecessary
            if (applicationCompositionSchemata.Count > 0) {
                var request = new PreFetchedCompositionFetchRequest(entities) {
                    CompositionList = new List<string>(applicationCompositionSchemata.Keys)
                };
                await GetCompositionData(application, request, null);
            }


            var listOptionsPrefetchRequest = new ListOptionsPrefetchRequest();
            var associationResults = await BuildAssociationOptions(DataMap.BlankInstance(application.Name), application.Schema, listOptionsPrefetchRequest);
            return new ApplicationListResult(totalCount, searchDto, entities, schema, associationResults) {
                AffectedProfiles = ctx.AvailableProfilesForGrid.Select(s => s.ToDTO()),
                CurrentSelectedProfile = ctx.CurrentSelectedProfile
            };
        }




        public virtual async Task<AssociationMainSchemaLoadResult> BuildAssociationOptions(AttributeHolder dataMap,
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

            ISet<string> handledAssociations = new HashSet<string>();

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

                tasks.Add(AssociationResolverWork(dataMap, schema, ctx, association, search, eagerFetchedOptions, lazyOptions));

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
                tasks.Add(DoResolveEagerOptions(schema, dataMap, optionField, eagerFetchedOptions));
            }
            #endregion



            await Task.WhenAll(tasks.ToArray());

            //let's handle eventual inline compositions afterwards to avoid an eventual thread explosion
            #region inlineCompositions

            if (Log.IsDebugEnabled) {
                var keys = string.Join(",", eagerFetchedOptions.Keys.Where(k => eagerFetchedOptions[k] != null)) + string.Join(",", lazyOptions.Keys.Where(k => lazyOptions[k] != null));
                Log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "Finished execution of options fetching. Resolved collections: {0}", keys));
            }

            result.EagerOptions = eagerFetchedOptions;
            result.PreFetchLazyOptions = lazyOptions;

            if (schema.HasInlineComposition && dataMap is CrudOperationData) {
                var innerCompositions = await GenerateInlineCompositionResult(dataMap, schema, request);
                result.MergeWithOtherSchemas(innerCompositions);
            }

            #endregion

            return result;
        }

        private async Task DoResolveEagerOptions(ApplicationSchemaDefinition schema, AttributeHolder dataMap, OptionField field, IDictionary<string, IEnumerable<IAssociationOption>>
                eagerFetchedOptions) {
            var associationOptions = await _dynamicOptionFieldResolver.ResolveOptions(schema, dataMap, field);
            eagerFetchedOptions.Add(field.AssociationKey, associationOptions);
        }


        private async Task AssociationResolverWork(AttributeHolder dataMap, ApplicationSchemaDefinition schema, object c,
            ApplicationAssociationDefinition association, SearchRequestDto search, IDictionary<string, IEnumerable<IAssociationOption>> eagerFetchedOptions,
            IDictionary<string, IDictionary<string, IAssociationOption>> lazyOptions) {
            Quartz.Util.LogicalThreadContext.SetData("context", c);
            var associationOptions = await _associationOptionResolver.ResolveOptions(schema, dataMap, association, search);
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
        }

        private async Task<List<AssociationMainSchemaLoadResult>> GenerateInlineCompositionResult(AttributeHolder dataMap, ApplicationSchemaDefinition schema,
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

                if (composition.Schema.Schemas == null) {
                    continue;
                }

                var listCompositionSchema = composition.Schema.Schemas.List;
                var compositionAssociations = listCompositionSchema.Associations(mode);

                if (compositionAssociations.Any()) {
                    var compositionData = (IEnumerable<CrudOperationData>)crudData.GetRelationship(composition.AssociationKey);
                    if (compositionData != null) {
                        var compositeDataMap = new CompositeDatamap(compositionData);
                        var compositionRequest = new InlineCompositionAssociationPrefetcherRequest(request, composition.AssociationKey);
                        var task = BuildAssociationOptions(compositeDataMap, listCompositionSchema, compositionRequest);
                        inlineCompositionTasks.Add(task);
                    }
                }
            }

            if (!inlineCompositionTasks.Any()) {
                return result;
            }

            await Task.WhenAll(inlineCompositionTasks.ToArray());
            foreach (var completedTask in inlineCompositionTasks) {
                result.Add(((Task<AssociationMainSchemaLoadResult>)completedTask).Result);
            }
            return result;
        }


        //        public virtual SynchronizationApplicationData Sync(ApplicationMetadata applicationMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData) {
        //            return Engine().Sync(applicationMetadata, applicationSyncData);
        //        }
        public virtual async Task<TargetResult> Execute(ApplicationMetadata application, JObject json, OperationDataRequest operationData) {
            var compositionData = operationData.CompositionData;
            if (compositionData == null || compositionData.Operation == null || !compositionData.Operation.EqualsAny(OperationConstants.CRUD_DELETE, OperationConstants.CRUD_UPDATE)) {
                //not a composition deletion/update, no need for any further checking
                return await Execute(application, json, operationData.Id, operationData.Operation, operationData.Batch,
                    new Tuple<string, string>(operationData.UserId, operationData.SiteId));
            }

            var clientComposition = compositionData.DispatcherComposition;
            var composition = application.Schema.Compositions().FirstOrDefault(f => f.Relationship.Equals(EntityUtil.GetRelationshipName(clientComposition)));
            if (composition == null) {
                return await Execute(application, json, operationData.Id, operationData.Operation, operationData.Batch,
                  new Tuple<string, string>(operationData.UserId, operationData.SiteId));
            }
            var compositionListSchema = composition.Schema.Schemas.List;
            var compositionEntityName = compositionListSchema.EntityName;
            var compositionEntity = MetadataProvider.Entity(compositionEntityName);
            var maximoWebServiceName = compositionEntity.ConnectorParameters.GetWSEntityKey();
            if (maximoWebServiceName == null) {
                //let parent web-service handle it
                return await Execute(application, json, operationData.Id, operationData.Operation, operationData.Batch,
                    new Tuple<string, string>(operationData.UserId, operationData.SiteId));
            }

            if (compositionEntity.ConnectorParameters.Parameters.ContainsKey("integration_interface_operations")) {
                var validOperations = compositionEntity.ConnectorParameters.Parameters["integration_interface_operations"].Split(',');
                if (!validOperations.Any(a => a.EqualsIc(compositionData.Operation))) {
                    //not to be handled by composed web-service either
                    return await Execute(application, json, operationData.Id, operationData.Operation, operationData.Batch,
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
                var targetResult = await ds.Execute(compositionApplication, GetCompositionJson(json, compositionData), compositionData.Id, compositionData.Operation,
                    operationData.Batch, new Tuple<string, string>(operationData.UserId, operationData.SiteId));

                //let's make sure the success message receives the right userId, which is the parent userid
                targetResult.UserId = operationData.UserId;

                return targetResult;
            }
            //otherwise let´s stick with the main app dataset
            return await Execute(compositionApplication, GetCompositionJson(json, compositionData), compositionData.Id, compositionData.Operation,
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

        public virtual async Task<TargetResult> Execute(ApplicationMetadata application, JObject json, string id, string operation, Boolean isBatch, Tuple<string, string> userIdSite) {
            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var operationWrapper = BuildOperationWrapper(application, json, id, operation, entityMetadata);
            if (userIdSite != null) {
                operationWrapper.UserId = userIdSite.Item1;
                operationWrapper.SiteId = userIdSite.Item2;
            }

            if (isBatch) {
                return BatchSubmissionService.CreateAndSubmit(operationWrapper.ApplicationMetadata.Name, operationWrapper.ApplicationMetadata.Schema.SchemaId, operationWrapper.JSON);
            }

            var result = await DoExecute(operationWrapper);
            var operationData = operationWrapper.OperationData();
            var crudOperationData = operationData as CrudOperationData;
            if (crudOperationData == null || crudOperationData.ReloadMode.Equals(ReloadMode.None)) {
                return result;
            }
            if (crudOperationData.ReloadMode.Equals(ReloadMode.FullRefresh)) {
                result.FullRefresh = true;
                return result;
            }
            if (OperationConstants.CRUD_CREATE.Equals(operation)) {
                id = result.Id;
                var siteId = result.SiteId ?? crudOperationData.SiteId;
                userIdSite = new Tuple<string, string>(result.UserId, siteId);
            }

            if (id == null && userIdSite == null) {
                return result;
            }

            //Main detail reload mode... full refresh would be handled at a higher level
            var slicedEntityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            var detailRequest = new DetailRequest();
            detailRequest.Key = application.Schema.GetSchemaKey();
            detailRequest.UserIdSitetuple = userIdSite;
            detailRequest.Id = id;
            var detailResult = await GetApplicationDetail(application, SecurityFacade.CurrentUser(), detailRequest);
            result.ResultObject = detailResult.ResultObject;
            return result;


        }

        protected virtual OperationWrapper BuildOperationWrapper(ApplicationMetadata application, JObject json, string id, string operation, EntityMetadata entityMetadata) {
            return new OperationWrapper(application, entityMetadata, operation, json, id);
        }

        #pragma warning disable 1998
        public virtual async Task<TargetResult> DoExecute(OperationWrapper operationWrapper) {
            return Engine().Execute(operationWrapper);
        }
        #pragma warning restore 1998

        public async Task<GenericResponseResult<IDictionary<string, BaseAssociationUpdateResult>>> UpdateAssociations(ApplicationMetadata application,
            AssociationUpdateRequest request, JObject currentData) {

            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var cruddata = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), entityMetadata,
                application, currentData, request.Id);
            //            if (EagerAssociationTrigger.Equals(request.TriggerFieldName)) {
            //                request.AssociationsToFetch = AssociationHelper.AllButSchema;
            //                return new GenericResponseResult<IDictionary<string, BaseAssociationUpdateResult>>(BuildAssociationOptions(cruddata, application, request));
            //            }
            return new GenericResponseResult<IDictionary<string, BaseAssociationUpdateResult>>(await DoUpdateAssociation(application, request, cruddata));
        }


        public virtual async Task<LookupOptionsFetchResultDTO> GetLookupOptions(ApplicationMetadata application, LookupOptionsFetchRequestDTO lookupRequest, AttributeHolder cruddata) {
            var before = LoggingUtil.StartMeasuring(Log, "fetching lookup options for application {0} schema {1}", application.Name, application.Schema.Name);

            var association = application.Schema.Associations().FirstOrDefault(f => (EntityUtil.IsRelationshipNameEquals(f.AssociationKey, lookupRequest.AssociationFieldName)));
            var associationApplicationMetadata = ApplicationAssociationResolver.GetAssociationApplicationMetadata(association);

            if (associationApplicationMetadata?.Schema?.SchemaFilters != null && lookupRequest.SearchDTO.AddPreSelectedFilters) {
                SchemaFilterBuilder.AddPreSelectedFilters(associationApplicationMetadata.Schema.SchemaFilters, lookupRequest.SearchDTO);
            }

            var options = await _associationOptionResolver.ResolveOptions(application.Schema, cruddata, association, lookupRequest.SearchDTO);

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
        protected virtual async Task<IDictionary<string, BaseAssociationUpdateResult>> DoUpdateAssociation(ApplicationMetadata application, AssociationUpdateRequest request,
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
                    tasks.Add(DoResolveOptionFields(application, cruddata, optionField, resultObject));

                } else {


                    var searchRequest = BaseDataSetSearchHelper.BuildSearchDTOForAssociationSearch(request, association, cruddata);

                    if (searchRequest == null) {
                        //this would only happen if association is lazy and there´s no default value 
                        //(cause we´d need to fetch one-value list for displaying)
                        continue;
                    }

                    tasks.Add(DoResolveOptionAssociations(application, cruddata, association, searchRequest, resultObject));
                }
            }

            await Task.WhenAll(tasks.ToArray());

            if (Log.IsDebugEnabled) {
                var keys = string.Join(",", resultObject.Keys.Where(k => resultObject[k].AssociationData != null));
                Log.Debug(LoggingUtil.BaseDurationMessageFormat(before,
                    "Finished execution of options fetching. Resolved collections: {0}", keys));
            }
            return resultObject;
        }

        private async Task DoResolveOptionFields(ApplicationMetadata application, AttributeHolder cruddata, OptionField optionField,
            IDictionary<string, BaseAssociationUpdateResult> resultObject) {
            var data = await _dynamicOptionFieldResolver.ResolveOptions(application.Schema, cruddata, optionField);
            if (data != null) {
                resultObject.Add(optionField.AssociationKey,
                    new LookupOptionsFetchResultDTO(data, 100, PaginatedSearchRequestDto.DefaultPaginationOptions));
            }
        }

        private async Task DoResolveOptionAssociations(ApplicationMetadata application, AttributeHolder cruddata, ApplicationAssociationDefinition association, PaginatedSearchRequestDto searchRequest,
         IDictionary<string, BaseAssociationUpdateResult> resultObject) {
            var associationApplicationMetadata =
                    ApplicationAssociationResolver.GetAssociationApplicationMetadata(association);
            var options = await _associationOptionResolver.ResolveOptions(application.Schema, cruddata, association,
                              searchRequest);

            resultObject.Add(association.AssociationKey,
                new LookupOptionsFetchResultDTO(searchRequest.TotalCount, searchRequest.PageNumber,
                    searchRequest.PageSize, options, associationApplicationMetadata));
        }


        public IEnumerable<IAssociationOption> GetSWPriorityType(OptionFieldProviderParameters parameters) {
            //create default priority list
            var list = new List<AssociationOption>
            {
                new AssociationOption("1", "1 - High"),
                new AssociationOption("2", "2 - Medium"),
                new AssociationOption("3", "3 - Low")
            };

            //list.Add(new AssociationOption("0", "0 - None"));

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
