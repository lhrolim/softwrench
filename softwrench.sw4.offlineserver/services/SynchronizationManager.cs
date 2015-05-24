using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using cts.commons.simpleinjector;
using cts.commons.Util;
using log4net;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Relational.Collection;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Metadata.Security;
using softwrench.sw4.offlineserver.dto;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
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
            var topLevelApps = GetTopLevelAppsToCollect(request,user);


            var result = new SynchronizationResultDto();

            IDictionary<CompleteApplicationMetadataDefinition, List<CompleteApplicationMetadataDefinition>> reverseCompositionMap
                = new Dictionary<CompleteApplicationMetadataDefinition, List<CompleteApplicationMetadataDefinition>>();

            foreach (var topLevelApp in topLevelApps) {
                ResolveApplication(request, user, topLevelApp, result, rowstampMap);
            }


            return result;
        }

        private void ResolveApplication(SynchronizationRequestDto request, InMemoryUser user, CompleteApplicationMetadataDefinition topLevelApp, SynchronizationResultDto result, JObject rowstampMap) {
            var watch = Stopwatch.StartNew();
            //this will return sync schema
            var userAppMetadata = topLevelApp.ApplyPolicies(ApplicationMetadataSchemaKey.GetSyncInstance(), user, ClientPlatform.Mobile);

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(userAppMetadata);
            var topLevelAppData = FetchData(entityMetadata, userAppMetadata);



            var appResultData = FilterData(topLevelApp.ApplicationName, topLevelAppData, rowstampMap);
            result.AddTopApplicationData(appResultData);
            Log.DebugFormat("SYNC:Finished handling top level app. Ellapsed {0}", LoggingUtil.MsDelta(watch));

            HandleCompositions(userAppMetadata, appResultData, result, rowstampMap);
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
        private void HandleCompositions(ApplicationMetadata topLevelApp, SynchronizationApplicationResultData appResultData, SynchronizationResultDto result, JObject rowstampMap)
        {
            var watch =Stopwatch.StartNew();
            
            var compositionMap = GetCompositionRowstampsDict(rowstampMap);

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

        private IEnumerable<CompleteApplicationMetadataDefinition> GetTopLevelAppsToCollect(SynchronizationRequestDto request,InMemoryUser user) {
            if (request.ApplicationName == null) {
                //no application in special was requested, lets return them all.
                return MetadataProvider.FetchTopLevelApps(ClientPlatform.Mobile,user);
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




        private SynchronizationApplicationResultData FilterData(string applicationName, ICollection<DataMap> topLevelAppData, JObject rowstampMap) {
            var watch = Stopwatch.StartNew();

            var result = new SynchronizationApplicationResultData {
                AllData = topLevelAppData
            };

            if (rowstampMap == null || rowstampMap.Count == 0) {
                //this should happen for first synchronization, of for "full-forced-synchronization"
                result.NewdataMaps = topLevelAppData;
                return result;
            }
            var idRowstampDict = ConvertJSONToDict(rowstampMap);
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

        public static IDictionary<string, string> ConvertJSONToDict(JObject rowstampMap) {
            if (rowstampMap == null || !rowstampMap.HasValues) {
                return new Dictionary<string, string>();
            }
            var result = new Dictionary<string, string>();
            dynamic obj = rowstampMap;
            //Loop over the array
            foreach (dynamic row in obj.items) {
                var id = row.id;
                var rowstamp = row.rowstamp;
                result.Add(id.Value, rowstamp.Value);
            }
            return result;
        }

        public static IDictionary<string, long?> GetCompositionRowstampsDict(JObject rowstampMap) {
            if (rowstampMap == null || !rowstampMap.HasValues) {
                return new Dictionary<string, long?>();
            }
            var result = new Dictionary<string, long?>();
            dynamic obj = rowstampMap;
            //Loop over the array
            foreach (dynamic row in obj.compositionmap) {
                var application = row.Name;
                result.Add(application, Int64.Parse(row.Value.Value));
            }
            return result;
        }

        private List<DataMap> FetchData(SlicedEntityMetadata entityMetadata, ApplicationMetadata appMetadata) {
            var enumerable = _repository.GetSynchronizationData(entityMetadata, new Rowstamps());
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
