using System.Collections.Generic;

namespace softwrench.sw4.offlineserver.dto.association {
    public class AssociationSynchronizationRequestDto : BaseSynchronizationRequestDto {
        public IList<string> ApplicationsToFetch { get; set; }

        /// <summary>
        /// List of applications that should not be queried/returned to the client side
        /// </summary>
        public IList<string> ApplicationsNotToFetch { get; set; }

        //these are the cache entries that do not need to be fetched again
        public ISet<string> CompleteCacheEntries { get; set; }

        public bool InitialLoad { get; set; }

    }
}