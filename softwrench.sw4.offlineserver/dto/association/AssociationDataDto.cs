using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using softWrench.sW4.Data;

namespace softwrench.sw4.offlineserver.dto.association {
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
        /// </summary>
        public ISet<string> CompleteCacheEntries { get; set; } = new HashSet<string>();


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
        }
    }
}
