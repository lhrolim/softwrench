using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using softwrench.sw4.api.classes.application;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.API.Association.Lookup;
using softWrench.sW4.Data.API.Association.SchemaLoading;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    /// <summary>
    /// marker interface for classes that wish to provide methods for the applications
    /// </summary>
    public interface IDataSet : IApplicationFiltereable, IComponent {
        Task<int>GetCount(ApplicationMetadata application, [CanBeNull]IDataRequest request);

        Task<IApplicationResponse> Get(ApplicationMetadata application, InMemoryUser user, IDataRequest request);

        Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request);
        Task<CompositionFetchResult> GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request, JObject currentData);
        Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto);

        Task<AssociationMainSchemaLoadResult> BuildAssociationOptions([NotNull]AttributeHolder dataMap, ApplicationSchemaDefinition schema, IAssociationPrefetcherRequest request);

        Task<LookupOptionsFetchResultDTO> GetLookupOptions(ApplicationMetadata application, LookupOptionsFetchRequestDTO lookupRequest, AttributeHolder cruddata);


        //        SynchronizationApplicationData Sync(ApplicationMetadata applicationMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData);
        TargetResult Execute(ApplicationMetadata application, JObject json, string id, string operation, Boolean isBatch,[CanBeNull]Tuple<string,string>userIdSite );

        TargetResult Execute(ApplicationMetadata application, JObject json, OperationDataRequest operationData);

        Task<GenericResponseResult<IDictionary<string, BaseAssociationUpdateResult>>> UpdateAssociations(ApplicationMetadata application,
            AssociationUpdateRequest request, JObject currentData);


    }
}