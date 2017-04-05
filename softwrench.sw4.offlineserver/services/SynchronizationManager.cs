﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using cts.commons.Util;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sw4.api.classes.application;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Metadata.Security;
using softwrench.sw4.offlineserver.dto;
using softwrench.sw4.offlineserver.dto.association;
using softwrench.sw4.offlineserver.events;
using softwrench.sw4.offlineserver.services.util;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;
using SynchronizationResultDto = softwrench.sw4.offlineserver.dto.SynchronizationResultDto;

namespace softwrench.sw4.offlineserver.services {
    public class SynchronizationManager : IBaseApplicationFiltereable, ISingletonComponent {

        private readonly OffLineCollectionResolver _resolver;
        private readonly EntityRepository _repository;
        private readonly IContextLookuper _lookuper;
        private readonly IEventDispatcher _iEventDispatcher;
        private readonly ISWDBHibernateDAO _swdbDAO;
        private readonly SyncChunkHandler _syncChunkHandler;
        private readonly IConfigurationFacade _configFacade;

        private static readonly ILog Log = LogManager.GetLogger(typeof(SynchronizationManager));

        public SynchronizationManager(OffLineCollectionResolver resolver, EntityRepository respository, IContextLookuper lookuper, IEventDispatcher iEventDispatcher, ISWDBHibernateDAO swdbDAO, SyncChunkHandler syncChunkHandler, IConfigurationFacade configFacade) {
            _resolver = resolver;
            _repository = respository;
            _lookuper = lookuper;
            _iEventDispatcher = iEventDispatcher;
            _swdbDAO = swdbDAO;
            _syncChunkHandler = syncChunkHandler;
            _configFacade = configFacade;
            Log.DebugFormat("init sync log");
        }


        public virtual async Task<SynchronizationResultDto> GetData(SynchronizationRequestDto request, InMemoryUser user) {
            _iEventDispatcher.Dispatch(new PreSyncEvent(request));

            var result = new SynchronizationResultDto { UserProperties = user.GenericSyncProperties };

            var rowstampMap = request.RowstampMap;
            var topLevelApps = GetTopLevelAppsToCollect(request, user);

            foreach (var topLevelApp in topLevelApps) {
                await ResolveApplication(request, user, topLevelApp, result, rowstampMap);
            }

            // add offline configs to the applications
            var configResultData = await GetOfflineConfigs();
            result.TopApplicationData.Add(configResultData);

            return result;
        }

        private async Task<SynchronizationApplicationResultData> GetOfflineConfigs() {
            var configs = await _swdbDAO.FindByQueryAsync<PropertyDefinition>("from PropertyDefinition where FullKey like '/Offline/%'");
            var datamaps = configs.Select(c => {
                var fields = new Dictionary<string, object>
                {
                    { "fullKey", c.FullKey },
                    { "stringValue" , c.StringValue},
                    { "defaultValue" , c.DefaultValue}
                };
                return new DataMap("_configuration", fields);
            }).ToList();

            return new SynchronizationApplicationResultData("_configuration") {
                InsertOrUpdateDataMaps = datamaps
            };
        }

        public virtual async Task<AssociationSynchronizationResultDto> GetAssociationData(InMemoryUser currentUser, AssociationSynchronizationRequestDto request) {
            _iEventDispatcher.Dispatch(new PreSyncEvent(request));

            var applicationsToFetch = BuildAssociationsToFetch(currentUser, request);
            var dict = ClientStateJsonConverter.GetAssociationRowstampDict(request.RowstampMap);
            return await DoGetAssociationData(applicationsToFetch, dict, currentUser);
        }

        protected virtual IEnumerable<CompleteApplicationMetadataDefinition> BuildAssociationsToFetch(InMemoryUser currentUser,
            AssociationSynchronizationRequestDto request) {
            IEnumerable<CompleteApplicationMetadataDefinition> applicationsToFetch;
            var applicationNamesToFetch = request.ApplicationsToFetch;
            if (applicationNamesToFetch == null || !applicationNamesToFetch.Any()) {
                //let´s bring all the associations
                applicationsToFetch = OffLineMetadataProvider.FetchAssociationApps(currentUser, true);
                if (request.ApplicationsNotToFetch != null) {
                    applicationsToFetch =
                        applicationsToFetch.Where(a => !request.ApplicationsNotToFetch.Contains(a.ApplicationName));
                }
            } else {
                applicationsToFetch = applicationNamesToFetch.Select(d => MetadataProvider.Application(d));
            }
            return applicationsToFetch;
        }

        protected virtual async Task<AssociationSynchronizationResultDto> DoGetAssociationData(IEnumerable<CompleteApplicationMetadataDefinition> applicationsToFetch,
            IDictionary<string, ClientAssociationCacheEntry> rowstampMap, InMemoryUser user) {
            var completeApplicationMetadataDefinitions = applicationsToFetch as CompleteApplicationMetadataDefinition[] ?? applicationsToFetch.ToArray();

            var maxThreads = await _configFacade.LookupAsync<int>(OfflineConstants.MaxAssociationThreads);

            var tasks = new Task[Math.Min(completeApplicationMetadataDefinitions.Count(), maxThreads)];
            var i = 0;
            var results = new AssociationSynchronizationResultDto();
            var watch = Stopwatch.StartNew();
            
            foreach (var association in completeApplicationMetadataDefinitions) {
                //this will return sync schema
                var userAppMetadata = association.ApplyPolicies(ApplicationMetadataSchemaKey.GetSyncInstance(), user, ClientPlatform.Mobile);

                Rowstamps rowstamp = null;
                if (rowstampMap.ContainsKey(association.ApplicationName)) {
                    var currentRowStamp = rowstampMap[association.ApplicationName].MaxRowstamp;
                    rowstamp = new Rowstamps(currentRowStamp, null);
                }
                if (i < maxThreads) {
                    tasks[i++] = InnerGetAssocData(association, userAppMetadata, rowstamp, results);
                } else {
                    //marking this association to be downloaded on a next chunk 
                    results.IncompleteAssociations.Add(association.ApplicationName);
                    results.HasMoreData = true;
                }

            }
            await Task.WhenAll(tasks);
            results = await _syncChunkHandler.HandleMaxSize(results);

            Log.DebugFormat("SYNC:Finished handling all associations. Ellapsed {0}", LoggingUtil.MsDelta(watch));

            return results;
        }



        protected virtual async Task InnerGetAssocData(CompleteApplicationMetadataDefinition association, ApplicationMetadata userAppMetadata, Rowstamps rowstamp, AssociationSynchronizationResultDto results) {
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(userAppMetadata);
            var context = _lookuper.LookupContext();
            context.OfflineMode = true;
            _lookuper.AddContext(context);
            var isLimited = association.GetProperty("mobile.fetchlimit") != null;
            results.LimitedAssociations.Add(userAppMetadata.Name, isLimited);

            var datamaps = await FetchData(entityMetadata, userAppMetadata, rowstamp, null, isLimited);
            results.AssociationData.Add(association.ApplicationName, datamaps);

            var textIndexes = new List<string>();
            results.TextIndexes.Add(association.ApplicationName, textIndexes);

            var numericIndexes = new List<string>();
            results.NumericIndexes.Add(association.ApplicationName, numericIndexes);

            var dateIndexes = new List<string>();
            results.DateIndexes.Add(association.ApplicationName, dateIndexes);

            ParseIndexes(textIndexes, numericIndexes, dateIndexes, association);
        }

        protected virtual async Task ResolveApplication(SynchronizationRequestDto request, InMemoryUser user, CompleteApplicationMetadataDefinition topLevelApp, SynchronizationResultDto result, JObject rowstampMap) {
            var watch = Stopwatch.StartNew();
            //this will return sync schema
            var userAppMetadata = topLevelApp.ApplyPolicies(ApplicationMetadataSchemaKey.GetSyncInstance(), user, ClientPlatform.Mobile);

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(userAppMetadata);
            var rowstampDTO = ClientStateJsonConverter.ConvertJSONToDict(rowstampMap);

            Rowstamps rowstamps = null;
            if (rowstampDTO.MaxRowstamp != null) {
                rowstamps = new Rowstamps(rowstampDTO.MaxRowstamp, null);
            }
            var isQuickSync = request.ItemsToDownload != null;

            var isLimited = topLevelApp.GetProperty("mobile.fetchlimit") != null;

            var topLevelAppData = await FetchData(entityMetadata, userAppMetadata, rowstamps, request.ItemsToDownload, isLimited);
            var appResultData = FilterData(topLevelApp.ApplicationName, topLevelAppData, rowstampDTO, topLevelApp, isQuickSync);

            result.AddTopApplicationData(appResultData);
            Log.DebugFormat("SYNC:Finished handling top level app. Ellapsed {0}", LoggingUtil.MsDelta(watch));

            if (!appResultData.IsEmptyExceptDeletion) {
                await HandleCompositions(userAppMetadata, appResultData, result, rowstampMap);
            }

        }

        /// <summary>
        /// Brings all composition data related to the main application passed as parameter (i.e only the ones whose parent entites are fetched).
        /// 
        /// If there´s no new parent entity, we can safely bring only compositions greater than the max rowstamp cached in the client side; 
        /// 
        /// If new entries appeared, however, for the sake of simplicity we won´t use this strategy since the whereclause might have changed in the process, causing loss of data
        /// 
        ///
        /// </summary>
        /// <param name="topLevelApp"></param>
        /// <param name="appResultData"></param>
        /// <param name="result"></param>
        /// <param name="rowstampMap"></param>
        protected virtual async Task HandleCompositions(ApplicationMetadata topLevelApp, SynchronizationApplicationResultData appResultData, SynchronizationResultDto result, JObject rowstampMap) {
            var watch = Stopwatch.StartNew();

            var compositionMap = ClientStateJsonConverter.GetCompositionRowstampsDict(rowstampMap);

            var parameters = new OffLineCollectionResolver.OfflineCollectionResolverParameters(topLevelApp, appResultData.AllData, compositionMap, appResultData.NewdataMaps, appResultData.AlreadyExistingDatamaps);

            if (!appResultData.AllData.Any()) {
                return;
            }
            var compositionData = await _resolver.ResolveCollections(parameters);



            foreach (var compositionDict in compositionData) {
                var dict = compositionDict;
                //lets assume no compositions can be updated, for the sake of simplicity
                var newDataMaps = new List<DataMap>();
                foreach (var list in compositionDict.Value.ResultList) {
                    newDataMaps.Add(new DataMap(dict.Key, list, dict.Value.IdFieldName));
                }

                result.AddCompositionData(new SynchronizationApplicationResultData(dict.Key, newDataMaps, null));
            }
            Log.DebugFormat("SYNC:Finished handling compositions. Ellapsed {0}", LoggingUtil.MsDelta(watch));

        }

        protected virtual IEnumerable<CompleteApplicationMetadataDefinition> GetTopLevelAppsToCollect(SynchronizationRequestDto request, InMemoryUser user) {
            if (request.ApplicationName == null) {
                //no application in special was requested, lets return them all.
                return MetadataProvider.FetchTopLevelApps(ClientPlatform.Mobile, user);
            }
            if (request.ReturnNewApps) {
                if (request.ClientCurrentTopLevelApps != null) {
                    var result = new HashSet<CompleteApplicationMetadataDefinition>();
                    var otherApps = MetadataProvider.FetchTopLevelApps(ClientPlatform.Mobile, user).Where(a => !request.ClientCurrentTopLevelApps.Contains(a.ApplicationName));
                    result.AddAll(otherApps);
                    result.Add(MetadataProvider.Application(request.ApplicationName));
                    return result;
                }
                return MetadataProvider.FetchTopLevelApps(ClientPlatform.Mobile, user);
            }



            return new List<CompleteApplicationMetadataDefinition> { MetadataProvider.Application(request.ApplicationName) };
        }




        protected virtual SynchronizationApplicationResultData FilterData(string applicationName, ICollection<DataMap> topLevelAppData, ClientStateJsonConverter.AppRowstampDTO rowstampDTO, CompleteApplicationMetadataDefinition topLevelApp, bool isQuickSync) {
            var watch = Stopwatch.StartNew();

            var result = new SynchronizationApplicationResultData(applicationName) {
                AllData = topLevelAppData,
            };

            ParseIndexes(result.TextIndexes, result.NumericIndexes, result.DateIndexes, topLevelApp);

            if (rowstampDTO.MaxRowstamp != null || isQuickSync) {
                //SWOFF-140 
                result.InsertOrUpdateDataMaps = topLevelAppData;
                return result;
            }

            //this is the full strategy implementation where the client passes the whole state on each synchronization
            var idRowstampDict = rowstampDTO.ClientState;

            if (idRowstampDict == null || idRowstampDict.Count == 0) {
                //this should happen for first synchronization, of for "full-forced-synchronization"
                result.NewdataMaps = topLevelAppData;
                return result;
            }
            //            var idRowstampDict = ClientStateJsonConverter.ConvertJSONToDict(rowstampMap);
            foreach (var dataMap in topLevelAppData) {
                var id = dataMap.Id;
                if (!idRowstampDict.ContainsKey(id)) {
                    Log.DebugFormat("sync: adding inserteable item {0} for application {1}", dataMap.Id, applicationName);
                    result.NewdataMaps.Add(dataMap);
                } else {
                    result.AlreadyExistingDatamaps.Add(dataMap);
                    var rowstamp = idRowstampDict[id];
                    if (!rowstamp.Equals(dataMap.Approwstamp.ToString())) {
                        Log.DebugFormat("sync: adding updateable item {0} for application {1}", dataMap.Id, applicationName);
                        result.UpdatedDataMaps.Add(dataMap);
                    }
                    //removing so that the remaining items are the deleted ids --> avoid an extra loop
                    idRowstampDict.Remove(id);
                }
            }

            Log.DebugFormat("sync: {0} items to delete for application {1}", result.DeletedRecordIds.Count(), applicationName);
            result.DeletedRecordIds = idRowstampDict.Keys;

            Log.DebugFormat("sync: filter data for {0} ellapsed {1}", applicationName, LoggingUtil.MsDelta(watch));
            return result;
        }

        private static void ParseIndexes(IList<string> textIndexes, IList<string> numericIndexes, IList<string> dateIndexes, CompleteApplicationMetadataDefinition topLevelApp) {
            var indexesString = topLevelApp.GetProperty(ApplicationSchemaPropertiesCatalog.ListOfflineTextIndexes);
            if (!string.IsNullOrEmpty(indexesString)) {
                indexesString.Split(',').ToList().ForEach(idx => ParseIndex(idx, textIndexes));
            }

            indexesString = topLevelApp.GetProperty(ApplicationSchemaPropertiesCatalog.ListOfflineNumericIndexes);
            if (!string.IsNullOrEmpty(indexesString)) {
                indexesString.Split(',').ToList().ForEach(idx => ParseIndex(idx, numericIndexes));
            }

            indexesString = topLevelApp.GetProperty(ApplicationSchemaPropertiesCatalog.ListOfflineDateIndexes);
            if (!string.IsNullOrEmpty(indexesString)) {
                indexesString.Split(',').ToList().ForEach(idx => ParseIndex(idx, dateIndexes));
            }
        }

        private static void ParseIndex(string index, IList<string> indexList) {
            var trimmed = index.Trim();
            if (string.IsNullOrEmpty(trimmed)) {
                return;
            }
            indexList.Add(trimmed);
        }

        protected virtual async Task<List<DataMap>> FetchData(SlicedEntityMetadata entityMetadata, ApplicationMetadata appMetadata, Rowstamps rowstamps = null, List<string> itemsToDownload = null, bool isLimited = false) {
            if (rowstamps == null) {
                rowstamps = new Rowstamps();
            }

            // no minimum rowstamp for sync -> sort by 'rowstamp' ascending
            //            var searchDto = string.IsNullOrWhiteSpace(rowstamps.Lowerlimit) ? new SearchRequestDto { SearchSort = "rowstamp asc" } : new SearchRequestDto();

            var searchDto = new SearchRequestDto {
                SearchSort = isLimited ? "rowstamp desc" : "rowstamp asc",
                Key = new ApplicationMetadataSchemaKey {
                    ApplicationName = appMetadata.Name
                }
            };


            if (itemsToDownload != null) {
                var notEmpty = itemsToDownload.Where(item => !string.IsNullOrEmpty(item)).ToList();
                if (!notEmpty.Any()) {
                    // QuickSync of created application - for now it's impossible a created application stay after sync
                    // TODO: fix the fetch of newly created application
                    searchDto.AppendWhereClause("1!=1");
                } else {
                    //ensure only the specified items are downloaded
                    searchDto.AppendWhereClauseFormat("{0} in ({1})", appMetadata.IdFieldName, BaseQueryUtil.GenerateInString(notEmpty));
                }
            }

            searchDto.QueryAlias = "sync:" + entityMetadata.Name;

            var enumerable = await _repository.GetSynchronizationData(entityMetadata, rowstamps, searchDto);
            if (!enumerable.Any()) {
                return new List<DataMap>();
            }
            var dataMaps = new List<DataMap>();
            foreach (var row in enumerable) {
                var dataMap = DataMap.Populate(appMetadata, row);
                dataMaps.Add(dataMap);
            }
            return dataMaps;
        }

        public string ApplicationName() {
            return null;
        }

        public string ClientFilter() {
            return null;
        }
    }
}
