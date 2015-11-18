using System;
using System.Collections.Generic;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using softwrench.sw4.api.classes.application;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
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
    public interface IDataSet : IApplicationFiltereable,IComponent {
        Int32 GetCount(ApplicationMetadata application, [CanBeNull]IDataRequest request);
        IApplicationResponse Get(ApplicationMetadata application, InMemoryUser user, IDataRequest request);
        ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request);
        CompositionFetchResult GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request, JObject currentData);
        ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto);
        AssociationMainSchemaLoadResult BuildAssociationOptions(AttributeHolder dataMap, ApplicationMetadata application, IAssociationPrefetcherRequest request);
//        SynchronizationApplicationData Sync(ApplicationMetadata applicationMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData);
        TargetResult Execute(ApplicationMetadata application, JObject json, string id, string operation,Boolean isBatch);

        GenericResponseResult<IDictionary<string, BaseAssociationUpdateResult>> UpdateAssociations(ApplicationMetadata application,
            AssociationUpdateRequest request, JObject currentData);

       
    }
}