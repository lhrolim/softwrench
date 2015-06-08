using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.offlineserver.dto.association {
    public class ClientAssociationCacheEntry {

        public String MaxRowstamp { get; set; }
        public String WhereClauseHash { get; set; }
        public String SyncSchemaHash { get; set; }

    }
}
