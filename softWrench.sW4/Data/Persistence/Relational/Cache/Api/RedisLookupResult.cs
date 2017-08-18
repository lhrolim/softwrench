using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.Persistence.Relational.Cache.Api {
    public class RedisLookupResult<T> where T: DataMap {

        public IList<RedisResultDTO<T>> Chunks { get; set; } = new List<RedisResultDTO<T>>();
        public ApplicationSchemaDefinition Schema { get; set; }

        /// <summary>
        /// Indicates whether there are more known chunks of cached data to be downloaded, even though the returned chunks were ignored due to the Global chunk Limit
        /// useful for specifying whether the database check may proceed
        /// </summary>
        public bool HasMoreChunks { get; set; }

        public long MaxRowstamp { get; set; }

        public IDictionary<string,CacheRoundtripStatus> ChunksAlreadyChecked { get; set; } = new Dictionary<string, CacheRoundtripStatus>();

        public ISet<string> ChunksIgnored { get; set; } = new HashSet<string>();

    }
}
