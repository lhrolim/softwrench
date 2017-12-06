using System;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;

namespace softwrench.sw4.umc.classes.com.cts.umc.service {
    public class UmcPmBatchService: ISingletonComponent {
        public static string PmCountQuery(string context) {
            if (ApplicationConfiguration.IsMSSQL(DBType.Swdb)) {
                return "CASE WHEN itemids = '' then 0 else len(itemids) - len(replace(itemids, ',', ''))+1 end";
            }
            if (ApplicationConfiguration.IsMySql()) {
                return "CASE WHEN itemids = '' then 0 else length(itemids) - length(replace(itemids, ',', ''))+1 end";
            }
            throw new NotSupportedException("db2 not yet implemented");
        }
    }
}
