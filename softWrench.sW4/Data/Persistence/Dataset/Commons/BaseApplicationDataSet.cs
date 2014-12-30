using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Security.Context;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

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

        protected readonly ILog Log = LogManager.GetLogger(typeof(BaseApplicationDataSet));

        private readonly ApplicationAssociationResolver _associationOptionResolver = new ApplicationAssociationResolver();
        private readonly DynamicOptionFieldResolver _dynamicOptionFieldResolver = new DynamicOptionFieldResolver();
        private readonly CollectionResolver _collectionResolver = new CollectionResolver();

        private IContextLookuper _contextLookuper;

        internal BaseApplicationDataSet() { }

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
            if (application.Schema.Stereotype == SchemaStereotype.Detail || request.Key.Mode == SchemaMode.input) {
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
            if (id != null) {
                dataMap = (DataMap)Engine().FindById(application.Schema, entityMetadata, id, applicationCompositionSchemas);
                if (request.InitialValues != null) {
                    var initValDataMap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application,
                        request.InitialValues, entityMetadata.Schema.MappingType);
                    dataMap = DefaultValuesBuilder.AddMissingInitialValues(dataMap, initValDataMap);
                }
            } else {
                dataMap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application, request.InitialValues, entityMetadata.Schema.MappingType);
            }
            var associationResults = BuildAssociationOptions(dataMap, application, request);
            var detailResult = new ApplicationDetailResult(dataMap, associationResults, application.Schema, applicationCompositionSchemas, id);
            return detailResult;
        }

        public CompositionFetchResult GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request, JObject currentData) {

            var applicationCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(application.Schema);
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);

            var cruddata = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), entityMetadata,
               application, currentData, request.Id);

            _collectionResolver.ResolveCollections(entityMetadata, applicationCompositionSchemas, cruddata);
            return new CompositionFetchResult(cruddata);
        }


        public virtual ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var totalCount = searchDto.TotalCount;
            IReadOnlyList<AttributeHolder> entities = null;

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            var schema = application.Schema;
            searchDto.BuildProjection(schema);
            var propertyValue = schema.GetProperty(ApplicationSchemaPropertiesCatalog.ListSchemaOrderBy);
            if (searchDto.SearchSort == null && propertyValue != null) {
                //if the schema has a default sort defined, and we didn´t especifally asked for any sort column, apply the default schema
                searchDto.SearchSort = propertyValue;
            }

            var tasks = new Task[2];
            var ctx = ContextLookuper.LookupContext();

            //count query
            tasks[0] = Task.Factory.NewThread(c => {
                var dto = searchDto.ShallowCopy();
                Quartz.Util.LogicalThreadContext.SetData("context", c);
                if (searchDto.NeedsCountUpdate) {
                    Log.DebugFormat("BaseApplicationDataSet#GetList calling Count method on maximo engine. Application Schema \"{0}\" / Context \"{1}\"", schema, c);
                    totalCount = Engine().Count(entityMetadata, dto);
                }
            }, ctx);

            //query
            tasks[1] = Task.Factory.NewThread(c => {
                var dto = searchDto.ShallowCopy();
                Quartz.Util.LogicalThreadContext.SetData("context", c);
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
                Log.DebugFormat("BaseApplicationDataSet#GetList calling Find method on maximo engine. Application Schema \"{0}\" / Context \"{1}\"", schema, c);
                entities = Engine().Find(entityMetadata, dto, applicationCompositionSchemata);
            }, ctx);

            Task.WaitAll(tasks);
            var listOptionsPrefetchRequest = new ListOptionsPrefetchRequest();
            var associationResults = BuildAssociationOptions(DataMap.BlankInstance(application.Name), application, listOptionsPrefetchRequest);
            return new ApplicationListResult(totalCount, searchDto, entities, schema, associationResults);
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

                //only resolve the association options for non lazy associations or (lazy loaded with value set or reverse associations)
                var search = new SearchRequestDto();
                if (!applicationAssociation.IsLazyLoaded()) {
                    // default branch
                } else if (dataMap != null && dataMap.GetAttribute(applicationAssociation.Target) != null) {
                    //if the field has a value, fetch only this single element, for showing eventual extra label fields... ==> lookup with a selected value
                    var toAttribute = applicationAssociation.EntityAssociation.PrimaryAttribute().To;
                    var prefilledValue = dataMap.GetAttribute(applicationAssociation.Target).ToString();
                    search.AppendSearchEntry(toAttribute, prefilledValue);
                }
                else if (dataMap != null && applicationAssociation.EntityAssociation.Reverse && dataMap.GetAttribute(applicationAssociation.EntityAssociation.PrimaryAttribute().From) != null)
                {
                    var toAttribute = applicationAssociation.EntityAssociation.PrimaryAttribute().To;
                    var prefilledValue = dataMap.GetAttribute(applicationAssociation.EntityAssociation.PrimaryAttribute().From).ToString();
                    search.AppendSearchEntry(toAttribute, prefilledValue);
                } else {
                    //lazy association with no default value
                    continue;
                }
                var association = applicationAssociation;

                tasks.Add(Task.Factory.NewThread(c => {
                    Quartz.Util.LogicalThreadContext.SetData("context", c);
                    var associationOptions = _associationOptionResolver.ResolveOptions(application, dataMap, association, search);
                    // update this line of code to return an empty array if associationOptions is null; associationOptions.ToArray will cause an compilation error.
                    // var associationData = associationOptions as IAssociationOption[] ?? associationOptions.ToArray();
                    var associationData = (associationOptions == null) ? new IAssociationOption[0] : associationOptions.ToArray();
                    associationOptionsDictionary.Add(association.AssociationKey, new BaseAssociationUpdateResult(associationData));
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
            return Engine().Sync(applicationMetadata, applicationSyncData);
        }

        public MaximoResult Execute(ApplicationMetadata application, JObject json, string id, string operation) {
            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var operationWrapper = new OperationWrapper(application, entityMetadata, operation, json, id);
            return Engine().Execute(operationWrapper);
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

                    var searchRequest = BaseDataSetSearchHelper.BuildSearchDTOForAssociationSearch(request, association, cruddata);

                    if (searchRequest == null) {
                        //this would only happen if association is lazy and there´s no default value 
                        //(cause we´d need to fetch one-value list for displaying)
                        continue;
                    }

                    tasks.Add(Task.Factory.NewThread(c => {
                        Quartz.Util.LogicalThreadContext.SetData("context", c);
                        var options = _associationOptionResolver.ResolveOptions(application, cruddata, association,
                            searchRequest);

                        resultObject.Add(association.AssociationKey,
                            new LookupAssociationUpdateResult(searchRequest.TotalCount, searchRequest.PageNumber,
                                searchRequest.PageSize, options, associationApplicationMetadata, PaginatedSearchRequestDto.DefaultPaginationOptions));
                    }, ctx));
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
