using System.Collections.Generic;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;

namespace softwrench.sw4.offlineserver.dto.association {
    public class AssociationSynchronizationRequestDto : BaseSynchronizationRequestDto {
        public IList<string> ApplicationsToFetch { get; set; }

        /// <summary>
        /// List of applications that should not be queried/returned to the client side
        /// </summary>
        public IList<string> ApplicationsNotToFetch { get; set; }

        //these are the cache entries that do not need to be fetched again
        public IDictionary<string, CacheRoundtripStatus> CompleteCacheEntries { get; set; } = new Dictionary<string, CacheRoundtripStatus>();

        public bool InitialLoad { get; set; }

    }
}