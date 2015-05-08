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
            var topLevelApps = GetTopLevelAppsToCollect(request);


            var result = new SynchronizationResultDto();

            IDictionary<CompleteApplicationMetadataDefinition, List<CompleteApplicationMetadataDefinition>> reverseCompositionMap
                = new Dictionary<CompleteApplicationMetadataDefinition, List<CompleteApplicationMetadataDefinition>>();

            foreach (var topLevelApp in topLevelApps) {
                ResolveApplication(request, user, topLevelApp, result, rowstampMap);
            }


            return result;
        }

        private void ResolveApplication(SynchronizationRequestDto request, InMemoryUser user, CompleteApplicationMetadataDefinition topLevelApp, SynchronizationResultDto result, JObject rowstampMap) {
            //this will return sync schema
            var userAppMetadata = topLevelApp.ApplyPolicies(ApplicationMetadataSchemaKey.GetSyncInstance(), user, ClientPlatform.Mobile);

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(userAppMetadata);
            var topLevelAppData = FetchData(entityMetadata, userAppMetadata);

            var parameters = new CollectionResolverParameters() {
                ApplicationMetadata = userAppMetadata,
                ParentEntities = topLevelAppData

            };
            var compositionData = _resolver.ResolveCollections(parameters);
            var filteredResults = FilterData(topLevelApp.ApplicationName, topLevelAppData, rowstampMap);
            var appResultData = new SynchronizationApplicationResultData(topLevelApp.ApplicationName, filteredResults.NewData, filteredResults.ChangedData, filteredResults.DeletedIds);

            result.AddTopApplicationData(appResultData);
            foreach (var compositionDict in compositionData) {
                var dict = compositionDict;
                //lets assume no compositions can be updated, for the sake of simplicity
                result.AddCompositionData(new SynchronizationApplicationResultData(compositionDict.Key, compositionDict.Value.ResultList.Select(s => new DataMap(dict.Key, s, dict.Value.IdFieldName)), null));
            }
        }

        private IEnumerable<CompleteApplicationMetadataDefinition> GetTopLevelAppsToCollect(SynchronizationRequestDto request) {
            if (request.ApplicationName == null) {
                //no application in special was requested, lets return them all.
                return OffLineMetadataProvider.FetchTopLevelApps();
            }
            if (request.ReturnNewApps) {
                if (request.ClientCurrentTopLevelApps != null) {
                    var result = new HashSet<CompleteApplicationMetadataDefinition>();
                    var otherApps = OffLineMetadataProvider.FetchTopLevelApps().Where(a => !request.ClientCurrentTopLevelApps.Contains(a.ApplicationName));
                    result.AddAll(otherApps);
                    result.Add(MetadataProvider.Application(request.ApplicationName));
                    return result;
                }
                return OffLineMetadataProvider.FetchTopLevelApps();
            }



            return new List<CompleteApplicationMetadataDefinition> { MetadataProvider.Application(request.ApplicationName) };
        }




        private ApplicationSyncResult FilterData(string applicationName, List<DataMap> topLevelAppData, JObject rowstampMap) {
            var watch = Stopwatch.StartNew();
            var result = new ApplicationSyncResult {
                AllData = topLevelAppData
            };

            if (rowstampMap == null || rowstampMap.Count == 0) {
                //this should happen for first synchronization, of for "full-forced-synchronization"
                return result;
            }
            var idRowstampDict = ConvertJSONToDict(rowstampMap);
            foreach (var dataMap in topLevelAppData) {
                var id = dataMap.Id;
                if (!idRowstampDict.ContainsKey(id)) {
                    Log.DebugFormat("sync: adding inserteable item {0} for application {1}", dataMap.Id, applicationName);
                    result.NewData.Add(dataMap);
                } else {
                    var rowstamp = idRowstampDict[id];
                    if (!Convert.ToInt64(rowstamp).Equals(dataMap.Approwstamp)) {
                        Log.DebugFormat("sync: adding updateable item {0} for application {1}", dataMap.Id, applicationName);
                        result.ChangedData.Add(dataMap);
                    }
                    //removing so that the remaining items are the deleted ids --> avoid an extra loop
                    idRowstampDict.Remove(id);
                }
            }

            Log.DebugFormat("sync: {0} items to delete for application {1}", result.DeletedIds.Count(), applicationName);
            result.DeletedIds = idRowstampDict.Keys;

            Log.DebugFormat("sync: filter data for {0} ellapsed {1}", applicationName, LoggingUtil.MsDelta(watch));
            return result;
        }

        public static IDictionary<string, string> ConvertJSONToDict(JObject rowstampMap) {
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

        class ApplicationSyncResult {



            internal List<DataMap> ChangedData = new List<DataMap>();
            internal readonly List<DataMap> NewData = new List<DataMap>();
            internal IEnumerable<String> DeletedIds = new List<string>();
            internal List<DataMap> AllData = new List<DataMap>();

        }
    }
}
