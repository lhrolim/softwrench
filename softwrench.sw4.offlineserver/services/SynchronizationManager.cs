using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using cts.commons.Util;
using Iesi.Collections.Generic;
using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json.Linq;
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
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using SynchronizationResultDto = softwrench.sw4.offlineserver.dto.SynchronizationResultDto;

namespace softwrench.sw4.offlineserver.services {
    public class SynchronizationManager : ISingletonComponent {

        private readonly OffLineCollectionResolver _resolver;
        private readonly EntityRepository _repository;
        private readonly IContextLookuper _lookuper;
        private readonly IEventDispatcher _iEventDispatcher;
        private readonly ISWDBHibernateDAO _swdbDAO;
        private readonly SyncChunkHandler _syncChunkHandler;
        private readonly IConfigurationFacade _configFacade;
        private readonly IRedisManager _redisManager;

        private static readonly ILog Log = LogManager.GetLogger(typeof(SynchronizationManager));

        public SynchronizationManager(OffLineCollectionResolver resolver, EntityRepository respository, IContextLookuper lookuper, IEventDispatcher iEventDispatcher, ISWDBHibernateDAO swdbDAO, SyncChunkHandler syncChunkHandler, IConfigurationFacade configFacade, IRedisManager redisManager) {
            _resolver = resolver;
            _repository = respository;
            _lookuper = lookuper;
            _iEventDispatcher = iEventDispatcher;
            _swdbDAO = swdbDAO;
            _syncChunkHandler = syncChunkHandler;
            _configFacade = configFacade;
            _redisManager = redisManager;
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
                return JSONConvertedDatamap.FromFieldsAndMappingType("_configuration", fields);
            }).ToList();

            return new SynchronizationApplicationResultData("_configuration") {
                InsertOrUpdateDataMaps = datamaps
            };
        }

        public virtual async Task<AssociationSynchronizationResultDto> GetAssociationData(InMemoryUser currentUser, AssociationSynchronizationRequestDto request) {
            _iEventDispatcher.Dispatch(new PreSyncEvent(request));

            var applicationsToFetch = BuildAssociationsToFetch(currentUser, request);
            var dict = ClientStateJsonConverter.GetAssociationRowstampDict(request.RowstampMap);
            return await DoGetAssociationData(applicationsToFetch, dict, currentUser, request.CompleteCacheEntries, request.InitialLoad);
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
            [NotNull] IDictionary<string, ClientAssociationCacheEntry> rowstampMap, InMemoryUser user, ISet<string> completeCacheEntries, bool initialLoad) {
            var applicationsArray = applicationsToFetch.ToArray();
            var completeApplicationMetadataDefinitions = applicationsToFetch as CompleteApplicationMetadataDefinition[] ?? applicationsArray;

            var maxThreads = await _configFacade.LookupAsync<int>(OfflineConstants.MaxAssociationThreads);
            var chunkLimit = await _configFacade.LookupAsync<int>(OfflineConstants.MaxDownloadSize);

            var results = new AssociationSynchronizationResultDto(chunkLimit);

            HandleIndexes(applicationsArray, results);

            if (initialLoad) {
                var cacheableApplications = completeApplicationMetadataDefinitions.Where(c => !"true".Equals(c.GetProperty(OfflineConstants.AvoidCaching))).ToList();
                var nonCacheableApplications = completeApplicationMetadataDefinitions.Where(c => "true".Equals(c.GetProperty(OfflineConstants.AvoidCaching))).ToList();
                foreach (var app in nonCacheableApplications) {
                    results.MarkAsIncomplete(app.ApplicationName);
                }

                await HandleCacheLookup(user, completeCacheEntries, cacheableApplications, maxThreads, results);
            }


            #region Database

            var watch = Stopwatch.StartNew();
            await HandleDatabase(initialLoad, rowstampMap, user, results, completeApplicationMetadataDefinitions, maxThreads);

            results = await _syncChunkHandler.HandleMaxSize(results);

            if (!results.IncompleteAssociations.Any()) {
                results.HasMoreData = false;
            }

            Log.DebugFormat("SYNC:Finished handling all associations. Ellapsed {0}", LoggingUtil.MsDelta(watch));

            #endregion

            return results;
        }

        private static Rowstamps BuildRowstamp(IDictionary<string, ClientAssociationCacheEntry> rowstampMap, AssociationSynchronizationResultDto results, CompleteApplicationMetadataDefinition association) {
            Rowstamps rowstamp = null;
            if (rowstampMap.ContainsKey(association.ApplicationName)) {
                var currentRowStamp = rowstampMap[association.ApplicationName].MaxRowstamp;
                rowstamp = new Rowstamps(currentRowStamp, null);
            } else if (results.AssociationData.ContainsKey(association.ApplicationName) && results
                           .AssociationData[association.ApplicationName]
                           .CachedMaxRowstamp.HasValue) {
                var cacheRowstamp = results.AssociationData[association.ApplicationName].CachedMaxRowstamp;
                rowstamp = new Rowstamps(cacheRowstamp.ToString(), null);
            }


            return rowstamp;
        }

        internal async Task HandleDatabase(bool initialLoad, IDictionary<string, ClientAssociationCacheEntry> rowstampMap, InMemoryUser user, AssociationSynchronizationResultDto results,
            IEnumerable<CompleteApplicationMetadataDefinition> completeApplicationMetadataDefinitions, int maxThreads) {

            var hasOverFlownOnCacheOperation = results.IsOverFlown();

            var applicationsToFetch = DatabaseApplicationsToCollect(initialLoad, results, completeApplicationMetadataDefinitions, hasOverFlownOnCacheOperation);

            var applicationMetadataDefinitions = applicationsToFetch as IList<CompleteApplicationMetadataDefinition> ?? applicationsToFetch.ToList();

            if (applicationMetadataDefinitions.Any()) {
                //after the cache has finished, regardless whether it has overflow, let´s force bringing automatically any values bigger than the cached rowstamp
                //for instance (cache brought all numericdomains until the rowstamp x --> now it´s time to query the eventual existing bigger rowstampped items)
                //these caches are way faster (due to rowstamp constraints), and shouldn´t return a meaningful number of results to impact the chunk size


                var tasks = new Task[Math.Min(applicationMetadataDefinitions.Count, maxThreads)];

                var j = 0;

                while (j < applicationMetadataDefinitions.Count) {

                    //if it has overflown, but not due to a cache hit
                    if (!hasOverFlownOnCacheOperation && results.IsOverFlown()) {
                        var association = applicationMetadataDefinitions[j++];
                        //marking this association to be downloaded on a next chunk 
                        results.MarkAsIncomplete(association.ApplicationName);
                        continue;
                    }


                    Log.DebugFormat("initing another round of database lookup, since we still have some space");

                    var i = 0;
                    var cacheTasks = new Task<RedisLookupResult<JSONConvertedDatamap>>[Math.Min(applicationMetadataDefinitions.Count - j, maxThreads)];

                    while (i < cacheTasks.Length && j < applicationMetadataDefinitions.Count) {
                        var association = applicationMetadataDefinitions[j++];
                        var userAppMetadata = association.ApplyPolicies(ApplicationMetadataSchemaKey.GetSyncInstance(),
                            user, ClientPlatform.Mobile);
                        var rowstamp = BuildRowstamp(rowstampMap, results, association);

                        tasks[i++] = InnerGetAssocData(association, userAppMetadata, rowstamp, results);
                    }

                    await Task.WhenAll(tasks);




                }
            } else {
                Log.DebugFormat("sync: Ingnoring association database queries, since the cache already fullfilled the chunk");
            }



        }

        //either all the incomplete applications, or, if the chunk limit was reached, only those where the cache lookup has exhausted
        internal virtual ISet<CompleteApplicationMetadataDefinition> DatabaseApplicationsToCollect(bool initialLoad, AssociationSynchronizationResultDto resultDTO, IEnumerable<CompleteApplicationMetadataDefinition> completeApplicationMetadataDefinitions, bool hasOverFlownOnCacheOperation) {

            var results = new HashSet<CompleteApplicationMetadataDefinition>();
            if (!initialLoad) {
                return new LinkedHashSet<CompleteApplicationMetadataDefinition>(results);
            }

            var definitions = completeApplicationMetadataDefinitions as IList<CompleteApplicationMetadataDefinition> ?? completeApplicationMetadataDefinitions.ToList();

            var nonCacheable = definitions.Where(c => "true".Equals(c.GetProperty(OfflineConstants.AvoidCaching)));

            if (hasOverFlownOnCacheOperation && resultDTO.HasMoreCacheData) {
                Log.DebugFormat("database operations will be ignored since there are still more cached entries to be collected");
                return results;
            }

            if (!hasOverFlownOnCacheOperation) {
                results.AddAll(nonCacheable);
            }

            //small datasets shall also be added, regardless of the main downloadchunk limit being overflown or not
            results.AddAll(nonCacheable.Where(c => "true".Equals(c.GetProperty(OfflineConstants.SmallDataSet))));


            if (hasOverFlownOnCacheOperation && resultDTO.CompleteCacheEntries.Any()) {
                //if the download chunk overflown, let´s return only the diferential database entries on top of the cache rowstamps
                //assuming there sould be few entries to collect, due to the rowstamp filtering
                results.AddAll(definitions.Where(
                    c => resultDTO.HasFinishedCollectingCache(c.ApplicationName) &&
                         ShouldCheckDatabaseAfterCache(c)));
            } else if (!resultDTO.HasMoreCacheData) {
                //there´s still space on the downloadchunk, and there are no more cache entries to check--> let´s fetch all the database entries 
                //for the apps which were not taken already from the cache
                results.AddAll(definitions.Where(c => !(resultDTO.HasFinishedCollectingCache(c.ApplicationName))));
            }

            return results;
        }

        protected virtual bool ShouldCheckDatabaseAfterCache(CompleteApplicationMetadataDefinition completeApplicationMetadataDefinition) {
            return "true".Equals(completeApplicationMetadataDefinition.GetProperty(OfflineConstants.CheckDatabaseAfterCache));
        }


        protected virtual async Task HandleCacheLookup(InMemoryUser user, ISet<string> completeCacheEntries,
            IList<CompleteApplicationMetadataDefinition> completeApplicationMetadataDefinitions, int maxThreads,
            AssociationSynchronizationResultDto results) {

            if (!_redisManager.IsAvailable()) {
                return;
            }

            var watch = Stopwatch.StartNew();

            var j = 0;


            Log.DebugFormat("init association cache lookup process");

            //until we reach the chunk limit, or we exhaust all the applications, let´s add more data into the results
            //limiting the threads to the specified configuration, to avoid a lack of resources on the IIS server
            while (j < completeApplicationMetadataDefinitions.Count) {

                if (results.IsOverFlown()) {
                    var association = completeApplicationMetadataDefinitions[j++];
                    results.MarkAsIncomplete(association.ApplicationName);
                    continue;
                }

                Log.DebugFormat("initing another round of cache lookup, since we still have some space");

                var i = 0;
                var cacheTasks = new Task<RedisLookupResult<JSONConvertedDatamap>>[Math.Min(completeApplicationMetadataDefinitions.Count - j, maxThreads)];

                while (i < cacheTasks.Length && j < completeApplicationMetadataDefinitions.Count) {
                    var association = completeApplicationMetadataDefinitions[j++];
                    var userAppMetadata = association.ApplyPolicies(ApplicationMetadataSchemaKey.GetSyncInstance(),
                        user, ClientPlatform.Mobile);
                    var lookupDTO = await BuildRedisDTO(userAppMetadata, completeCacheEntries);
                    cacheTasks[i++] = _redisManager.Lookup<JSONConvertedDatamap>(lookupDTO);
                }

                var redisResults = await Task.WhenAll(cacheTasks);
                foreach (var redisResult in redisResults) {
                    //each application might span multiple chunks of data, although it would be abnormal (ex: change in the chunk size)
                    results.AddJsonFromRedisResult(redisResult, "true".Equals(redisResult.Schema.GetProperty(OfflineConstants.CheckDatabaseAfterCache)));
                }
            }

            Log.InfoFormat("Cache lookup profess completed in {0}", LoggingUtil.MsDelta(watch));

        }


        protected virtual async Task InnerGetAssocData(CompleteApplicationMetadataDefinition association, ApplicationMetadata userAppMetadata, Rowstamps rowstamp, AssociationSynchronizationResultDto results) {
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(userAppMetadata);
            var context = _lookuper.LookupContext();
            context.OfflineMode = true;
            _lookuper.AddContext(context);
            var isLimited = association.GetProperty("mobile.fetchlimit") != null;
            results.LimitedAssociations.Add(userAppMetadata.Name, isLimited);

            var datamaps = await FetchData(true, entityMetadata, userAppMetadata, rowstamp, null, isLimited);
            results.AddIndividualJsonDatamaps(association.ApplicationName, datamaps);
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

            var topLevelAppData = await FetchData(false, entityMetadata, userAppMetadata, rowstamps, request.ItemsToDownload, isLimited);


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

            var parameters = new OffLineCollectionResolver.OfflineCollectionResolverParameters(topLevelApp, appResultData.AllData, compositionMap, appResultData.NewdataMaps.Select(s => s.OriginalDatamap), appResultData.AlreadyExistingDatamaps);

            if (!appResultData.AllData.Any()) {
                return;
            }
            var compositionData = await _resolver.ResolveCollections(parameters);



            foreach (var compositionDict in compositionData) {
                var dict = compositionDict;
                //lets assume no compositions can be updated, for the sake of simplicity
                var newDataMaps = new List<JSONConvertedDatamap>();
                foreach (var list in compositionDict.Value.ResultList) {
                    newDataMaps.Add(JSONConvertedDatamap.FromFields(dict.Key, list, dict.Value.IdFieldName));
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





        protected virtual SynchronizationApplicationResultData FilterData(string applicationName, ICollection<JSONConvertedDatamap> topLevelAppData, ClientStateJsonConverter.AppRowstampDTO rowstampDTO, CompleteApplicationMetadataDefinition topLevelApp, bool isQuickSync) {
            var watch = Stopwatch.StartNew();

            var result = new SynchronizationApplicationResultData(applicationName) {
                AllData = topLevelAppData.Select(t => t.OriginalDatamap).ToList(),
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
                    result.AlreadyExistingDatamaps.Add(dataMap.OriginalDatamap);
                    var rowstamp = idRowstampDict[id];
                    if (!rowstamp.Equals(dataMap.Approwstamp.ToString())) {
                        Log.DebugFormat("sync: adding updateable item {0} for application {1}", dataMap.Id, applicationName);
                        result.UpdatedDataMaps.Add(dataMap);
                    }
                    //removing so that the remaining items are the deleted ids --> avoid an extra loop
                    idRowstampDict.Remove(id);
                }
            }

            Log.DebugFormat("sync: {0} items to delete for application {1}", result.DeletedRecordIds.Count, applicationName);
            result.DeletedRecordIds = idRowstampDict.Keys;

            Log.DebugFormat("sync: filter data for {0} ellapsed {1}", applicationName, LoggingUtil.MsDelta(watch));
            return result;
        }

        private static void HandleIndexes(IEnumerable<CompleteApplicationMetadataDefinition> associations, AssociationSynchronizationResultDto results) {
            associations?.ToList().ForEach(association => {
                var textIndexes = new List<string>();
                results.TextIndexes.Add(association.ApplicationName, textIndexes);

                var numericIndexes = new List<string>();
                results.NumericIndexes.Add(association.ApplicationName, numericIndexes);

                var dateIndexes = new List<string>();
                results.DateIndexes.Add(association.ApplicationName, dateIndexes);

                ParseIndexes(textIndexes, numericIndexes, dateIndexes, association);
            });
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


        protected virtual async Task<List<JSONConvertedDatamap>> FetchData(bool isAssociationData, SlicedEntityMetadata entityMetadata, ApplicationMetadata appMetadata,
            Rowstamps rowstamps = null, List<string> itemsToDownload = null, bool isLimited = false) {

            var searchDto = new SearchRequestDto {
                SearchSort = isLimited ? "rowstamp desc" : "rowstamp asc",
                Key = new ApplicationMetadataSchemaKey {
                    ApplicationName = appMetadata.Name
                }
            };

            if (isAssociationData && rowstamps == null) {
                //initial asociation download, using uid ascending, so that we can cache the results if needed be
                searchDto.SearchSort = "{0} {1}".Fmt(appMetadata.Schema.IdFieldName, isLimited ? "desc" : "asc");
            }

            if (rowstamps == null) {
                rowstamps = new Rowstamps();
            }




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
                return new List<JSONConvertedDatamap>();
            }
            var dataMaps = new List<JSONConvertedDatamap>();
            foreach (var row in enumerable) {
                var dataMap = DataMap.Populate(appMetadata, row);
                dataMaps.Add(new JSONConvertedDatamap(dataMap));
            }
            if (isAssociationData && !"true".Equals(appMetadata.Schema.GetProperty(OfflineConstants.AvoidCaching))) {

                //updating cache entries

                var lookupDTO = await BuildRedisDTO(appMetadata, null);
                var inputDTO = new RedisInputDTO<JSONConvertedDatamap>(dataMaps);
#pragma warning disable 4014
                //updating cache on background thread
                Task.Run(() => _redisManager.InsertIntoCache(lookupDTO, inputDTO));
#pragma warning restore 4014
            }

            return dataMaps;
        }

        protected virtual async Task<RedisLookupDTO> BuildRedisDTO(ApplicationMetadata appMetadata, ISet<string> completeCacheEntries) {

            var maxSize = await _configFacade.LookupAsync<int>(OfflineConstants.MaxDownloadSize);
            var user = SecurityFacade.CurrentUser();

            var lookupDTO = new RedisLookupDTO {
                Schema = appMetadata.Schema,
                IsOffline = true,
                GlobalLimit = maxSize,
                CacheEntriesToAvoid = completeCacheEntries
            };
            lookupDTO.ExtraKeys.Add("siteid", user.SiteId);
            lookupDTO.ExtraKeys.Add("orgid", user.OrgId);

            return lookupDTO;
        }


    }
}
