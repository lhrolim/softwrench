using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.Util;
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
using softwrench.sw4.offlineserver.services.util;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Util;
using SynchronizationResultDto = softwrench.sw4.offlineserver.dto.SynchronizationResultDto;

namespace softwrench.sw4.offlineserver.services {
    public class SynchronizationManager : ISingletonComponent {

        private readonly OffLineCollectionResolver _resolver;
        private readonly EntityRepository _repository;

        private static readonly ILog Log = LogManager.GetLogger(typeof(SynchronizationManager));

        public SynchronizationManager(OffLineCollectionResolver resolver, EntityRepository respository) {
            _resolver = resolver;
            _repository = respository;
            Log.DebugFormat("init sync log");
        }


        public SynchronizationResultDto GetData(SynchronizationRequestDto request, InMemoryUser user, JObject rowstampMap) {
            var topLevelApps = GetTopLevelAppsToCollect(request, user);

            var result = new SynchronizationResultDto();

            foreach (var topLevelApp in topLevelApps) {
                ResolveApplication(request, user, topLevelApp, result, rowstampMap);
            }

            return result;
        }

        public AssociationSynchronizationResultDto GetAssociationData(InMemoryUser currentUser, JObject rowstampMap, string applicationToFetch = null) {
            var watch = Stopwatch.StartNew();
            IEnumerable<CompleteApplicationMetadataDefinition> applicationsToFetch;
            if (applicationToFetch == null) {
                //let´s bring all the associations
                applicationsToFetch = OffLineMetadataProvider.FetchAssociationApps(currentUser,true);
            } else {
                var app = MetadataProvider.Application(applicationToFetch);
                applicationsToFetch = new List<CompleteApplicationMetadataDefinition>() { app };
            }
            var dict = ClientStateJsonConverter.GetAssociationRowstampDict(rowstampMap);
            return DoGetAssociationData(applicationsToFetch, dict, currentUser);
        }

        private AssociationSynchronizationResultDto DoGetAssociationData(IEnumerable<CompleteApplicationMetadataDefinition> applicationsToFetch, IDictionary<string, ClientAssociationCacheEntry> rowstampMap, InMemoryUser user) {
            var completeApplicationMetadataDefinitions = applicationsToFetch as CompleteApplicationMetadataDefinition[] ?? applicationsToFetch.ToArray();

            var tasks = new Task[completeApplicationMetadataDefinitions.Count()];
            var i = 0;
            var results = new AssociationSynchronizationResultDto();
            var watch = Stopwatch.StartNew();
            foreach (var association in completeApplicationMetadataDefinitions) {

                //this will return sync schema
                var userAppMetadata = association.ApplyPolicies(ApplicationMetadataSchemaKey.GetSyncInstance(), user, ClientPlatform.Mobile);

                var entityMetadata = MetadataProvider.SlicedEntityMetadata(userAppMetadata);
                var association1 = association;

                Rowstamps rowstamp = null;
                if (rowstampMap.ContainsKey(association.ApplicationName)) {
                    var currentRowStamp = rowstampMap[association.ApplicationName].MaxRowstamp;
                    rowstamp = new Rowstamps(currentRowStamp, null);
                }

                tasks[i++] = Task.Factory.NewThread(() => {
                    var datamaps = FetchData(entityMetadata, userAppMetadata, rowstamp);
                    results.AssociationData.Add(association1.ApplicationName, datamaps);
                });

            }
            Task.WaitAll(tasks);
            Log.DebugFormat("SYNC:Finished handling all associations. Ellapsed {0}", LoggingUtil.MsDelta(watch));

            return results;
        }

        private void ResolveApplication(SynchronizationRequestDto request, InMemoryUser user, CompleteApplicationMetadataDefinition topLevelApp, SynchronizationResultDto result, JObject rowstampMap) {
            var watch = Stopwatch.StartNew();
            //this will return sync schema
            var userAppMetadata = topLevelApp.ApplyPolicies(ApplicationMetadataSchemaKey.GetSyncInstance(), user, ClientPlatform.Mobile);

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(userAppMetadata);
            var rowstampDTO = ClientStateJsonConverter.ConvertJSONToDict(rowstampMap);

            Rowstamps rowstamps = null;
            if (rowstampDTO.MaxRowstamp != null) {
                rowstamps = new Rowstamps(rowstampDTO.MaxRowstamp, null);
            }

            var topLevelAppData = FetchData(entityMetadata, userAppMetadata, rowstamps);
            var appResultData = FilterData(topLevelApp.ApplicationName, topLevelAppData, rowstampDTO);

            result.AddTopApplicationData(appResultData);
            Log.DebugFormat("SYNC:Finished handling top level app. Ellapsed {0}", LoggingUtil.MsDelta(watch));

            if (!appResultData.IsEmpty) {
                HandleCompositions(userAppMetadata, appResultData, result, rowstampMap);
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
        private void HandleCompositions(ApplicationMetadata topLevelApp, SynchronizationApplicationResultData appResultData, SynchronizationResultDto result, JObject rowstampMap) {
            var watch = Stopwatch.StartNew();

            var compositionMap = ClientStateJsonConverter.GetCompositionRowstampsDict(rowstampMap);

            var parameters = new OffLineCollectionResolver.OfflineCollectionResolverParameters(topLevelApp, appResultData.AllData, compositionMap, appResultData.NewdataMaps, appResultData.AlreadyExistingDatamaps);
            var compositionData = _resolver.ResolveCollections(parameters);



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

        private IEnumerable<CompleteApplicationMetadataDefinition> GetTopLevelAppsToCollect(SynchronizationRequestDto request, InMemoryUser user) {
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




        private SynchronizationApplicationResultData FilterData(string applicationName, ICollection<DataMap> topLevelAppData, ClientStateJsonConverter.AppRowstampDTO rowstampDTO) {
            var watch = Stopwatch.StartNew();

            var result = new SynchronizationApplicationResultData {
                AllData = topLevelAppData
            };

            if (rowstampDTO.MaxRowstamp != null) {
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



        private List<DataMap> FetchData(SlicedEntityMetadata entityMetadata, ApplicationMetadata appMetadata, Rowstamps rowstamps = null) {
            if (rowstamps == null) {
                rowstamps = new Rowstamps();
            }

            // no minimum rowstamp for sync -> sort by 'rowstamp' descending
            var searchDto = string.IsNullOrWhiteSpace(rowstamps.Lowerlimit) ? new SearchRequestDto() { SearchSort = "rowstamp desc" } : new SearchRequestDto();
            searchDto.Key = new ApplicationMetadataSchemaKey() {
                ApplicationName = appMetadata.Name
            };

            var enumerable = _repository.GetSynchronizationData(entityMetadata, rowstamps, searchDto);
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



    }
}
