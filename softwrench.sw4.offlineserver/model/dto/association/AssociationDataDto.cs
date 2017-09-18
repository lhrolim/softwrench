using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;

namespace softwrench.sw4.offlineserver.model.dto.association {
    public class AssociationDataDto {

        public List<DataMap> IndividualItems = new List<DataMap>();

        //        //to avoid extra round trip serializations, assuming the whole list is stored as a cached json
        //        public string RawJson { get; set; }
        //
        //        public int RawJsonCount { get; set; }

        [JsonIgnore]
        public long? CachedMaxRowstamp { get; set; }

        /// <summary>
        /// whether we know for sure that there are more cahed entries to be collected for this particular association. If this is set to true, we won´t collect any database entries
        /// </summary>
        [JsonIgnore]
        public bool HasMoreCachedEntries { get; set; }


        /// <summary>
        /// There´s still more data available to be downloaded, either from the RedisCache or from the Database.
        /// 
        /// The framework will determine it based upon the CompleteCacheEntries Set for the next round of the synchronization Roundtrip
        /// 
        /// </summary>
        public bool Incomplete { get; set; }

        /// <summary>
        /// The ids of the cache entries that were already checked and returned, and hence do not need to be looked up again;
        /// 
        /// This avoids an eventual deserialization cost that would take place to limit the chunk of the json data that lies on the in memory caches
        /// 
        ///  k = cache key
        ///  v (bool) = true if this cache entry was already checked on a previous roundtrip sync, false otherwise (i.e subject of this roundtrip check)
        /// 
        /// </summary>
        public IDictionary<string, CacheRoundtripStatus> CompleteCacheEntries { get; set; } = new Dictionary<string, CacheRoundtripStatus>();


        //        /// <summary>
        //        /// The ids of the cache entries that were already checked and returned, and hence do not need to be looked up again;
        //        /// 
        //        /// This avoids an eventual deserialization cost that would take place to limit the chunk of the json data that lies on the in memory caches
        //        /// 
        //        /// </summary>
        //        public ISet<string> ThisRoundTripCompleteCacheEntries { get; set; } = new HashSet<string>();

        /// <summary>
        /// Set true when no results/chunks were found for the association on cache
        /// TODO: analize the necessity of this cachemiss or if its better change the Incomplete flag to an enum
        /// </summary>
        public bool CacheMiss { get; set; }


        /// <summary>
        /// Name of the remoteid to be used as an unique index
        /// </summary>
        public string RemoteIdFieldName { get; set; }


        public bool Any() {
            return IndividualItems.Any();
        }

        public int Count => IndividualItems.Count;




        public void Skip(int max) {
            IndividualItems = IndividualItems.Skip(max).ToList();
        }

        public void Take(int max) {
            IndividualItems = IndividualItems.Take(max).ToList();
        }

        public void Clear() {
            IndividualItems.Clear();
            foreach (var cacheEntry in CompleteCacheEntries) {
                if (!cacheEntry.Value.Complete){
                    cacheEntry.Value.TransientPosition = 0;
                }
            }
        }

        public override string ToString() {
            return $"{nameof(HasMoreCachedEntries)}: {HasMoreCachedEntries}, {nameof(Count)}: {Count}";
        }
    }
}
