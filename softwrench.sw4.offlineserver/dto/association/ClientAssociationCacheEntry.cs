using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.offlineserver.dto.association {
    public class ClientAssociationCacheEntry {

        public string MaxRowstamp { get; set; }
        public string WhereClauseHash { get; set; }
        public string SyncSchemaHash { get; set; }

    }
}
