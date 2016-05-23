using System.Collections.Generic;

namespace softWrench.sW4.Configuration.Services {
    public class ClientSideConfigurations {
        public IDictionary<string, string> Configurations { get; set; }
        public long CacheTimestamp { get; set; }
    }
}
