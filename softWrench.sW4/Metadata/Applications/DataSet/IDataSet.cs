﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Data;
using IComponent = softWrench.sW4.SimpleInjector.IComponent;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    /// <summary>
    /// marker interface for classes that wish to provide methods for the applications
    /// </summary>
    public interface IDataSet : IComponent {
        Int32 GetCount(ApplicationMetadata application, [CanBeNull]IDataRequest request);
        IApplicationResponse Get(ApplicationMetadata application, InMemoryUser user, IDataRequest request);
        ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request);
        CompositionFetchResult GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request, JObject currentData);
        ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto);
        IDictionary<string, BaseAssociationUpdateResult> BuildAssociationOptions(AttributeHolder dataMap, ApplicationMetadata application, IAssociationPrefetcherRequest request);
        SynchronizationApplicationData Sync(ApplicationMetadata applicationMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData);
        MaximoResult Execute(ApplicationMetadata application, JObject json, string id, string operation);

        GenericResponseResult<IDictionary<string, BaseAssociationUpdateResult>> UpdateAssociations(ApplicationMetadata application,
            AssociationUpdateRequest request, JObject currentData);

        string ApplicationName();
        string ClientFilter();
    }
}