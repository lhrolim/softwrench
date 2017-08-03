using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Relational.Cache;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;
using softWrench.sW4.Util;

namespace softwrench.sw4.offlineserver.dto.association {

    public class AssociationSynchronizationResultDto {


        public IDictionary<string, AssociationDataDto> AssociationData { get; set; }

        public ISet<string> IncompleteAssociations {
            get { return new HashSet<string>(AssociationData.Where(a => a.Value.Incomplete).Select(k => k.Key)); }
        }

        [JsonIgnore]
        public ISet<string> AssociationsWithData {
            get { return new HashSet<string>(AssociationData.Where(a => a.Value.IndividualItems.Any()).Select(k => k.Key)); }
        }

        public ISet<string> CompleteCacheEntries {
            get { return new HashSet<string>(AssociationData.Values.SelectMany(v => v.CompleteCacheEntries)); }
        }

        [JsonIgnore]
        public int TotalCount {
            get {
                var sum = 0;

                foreach (var data in AssociationData.Values) {
                    sum += data.Count;
                }

                return sum;
            }
        }

        [JsonIgnore]
        public IDictionary<string, bool> LimitedAssociations { get; set; } = new ConcurrentDictionary<string, bool>();

        public IDictionary<string, IList<string>> TextIndexes { get; set; }
        public IDictionary<string, IList<string>> NumericIndexes { get; set; }
        public IDictionary<string, IList<string>> DateIndexes { get; set; }

        private int ChunkLimit { get; set; }

        /// <summary>
        /// If true there´s more data to be downloaded on the syncoperation, used to force a sync in chunks at the client side to avoid eventual memory issues with large datasets
        /// </summary>
        public bool HasMoreData { get; set; }

        public AssociationSynchronizationResultDto(int chunkLimit) {
            AssociationData = new ConcurrentDictionary<string, AssociationDataDto>();
            TextIndexes = new ConcurrentDictionary<string, IList<string>>();
            NumericIndexes = new ConcurrentDictionary<string, IList<string>>();
            DateIndexes = new ConcurrentDictionary<string, IList<string>>();
            ChunkLimit = chunkLimit;
        }

        public bool IsEmpty {
            get { return AssociationData.All(data => !data.Value.Any()); }
        }

        public bool HasMoreCacheData {
            get { return AssociationData.Values.Any(v => v.HasMoreCachedEntries); }
        }

        public bool IsOverFlown() {
            var sum = 0;
            foreach (var data in AssociationData) {
                sum += data.Value.Count;
            }
            return sum >= ChunkLimit;
        }

        public void AddIndividualDatamaps(string key, List<DataMap> datamaps) {
            AssociationData[key] = new AssociationDataDto {
                IndividualItems = datamaps
            };
        }

        public void AddIndividualJsonDatamaps(string key,string idFieldName, IList<JSONConvertedDatamap> datamaps) {
            if (!AssociationData.ContainsKey(key)) {
                AssociationData[key] = new AssociationDataDto();
            }

            var associationData = AssociationData[key];
            foreach (var dm in datamaps) {
                associationData.IndividualItems.Add(dm);
            }
            //marking as incomplete until otherwise stated at the SyncChunkHandler
            //TODO: create extra flags
            associationData.RemoteIdFieldName = idFieldName;
            associationData.Incomplete = false;
        }

        public void MarkAsIncomplete(string applicationName) {
            AssociationData[applicationName] = new AssociationDataDto { Incomplete = true };
            HasMoreData = true;
        }

        public void AddJsonFromRedisResult<T>(RedisLookupResult<T> redisResult, bool shouldCheckDB) where T : DataMap {
            if (!redisResult.Chunks.Any()) {
                AssociationData[redisResult.Schema.ApplicationName] = new AssociationDataDto { Incomplete = true };
                return;
            }


            var associationDataDto = new AssociationDataDto();
            //these were already downloaded on other chunks
            associationDataDto.CompleteCacheEntries.AddAll(redisResult.ChunksAlreadyChecked);
            associationDataDto.HasMoreCachedEntries = redisResult.HasMoreChunks;

            //marking entries as incomplete to force a database synchronization based on the maxrowstamp
            associationDataDto.Incomplete = shouldCheckDB || redisResult.HasMoreChunks;
            associationDataDto.CachedMaxRowstamp = redisResult.MaxRowstamp;

            foreach (var chunk in redisResult.Chunks) {

                foreach (var dm in chunk.Results) {
                    associationDataDto.IndividualItems.Add(dm);
                }

                associationDataDto.CompleteCacheEntries.Add(chunk.RealKey);

            }
            associationDataDto.RemoteIdFieldName = redisResult.Schema.IdFieldName;
            AssociationData[redisResult.Schema.ApplicationName] = associationDataDto;

            HasMoreData = true;



        }

        public override string ToString() {
            return $"{nameof(AssociationData)}: {AssociationData.Count + ":" + TotalCount}, {nameof(HasMoreData)}: {HasMoreData}, {nameof(IsEmpty)}: {IsEmpty}";
        }

        public bool HasFinishedCollectingCache(string applicationName) {
            if (!AssociationData.ContainsKey(applicationName)) {
                return false;
            }

            var associationData = AssociationData[applicationName];
            return associationData.CompleteCacheEntries.Any() && !associationData.HasMoreCachedEntries;
        }
    }
}
