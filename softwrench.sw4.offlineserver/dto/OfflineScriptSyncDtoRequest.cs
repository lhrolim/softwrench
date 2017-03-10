using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Dynamic.Model;

namespace softwrench.sw4.offlineserver.dto {
    public class OfflineScriptSyncDtoRequest {

        public IDictionary<string, long> ClientState { get; set; }

        public string offlineVersion { get; set; }

        public OfflineDevice offlineDevice { get; set; }

    }
}
