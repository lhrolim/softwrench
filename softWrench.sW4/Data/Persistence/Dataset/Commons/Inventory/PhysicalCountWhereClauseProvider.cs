using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Inventory {
    public class PhysicalCountWhereClauseProvider : ISingletonComponent {

        public static string PhysicalCountQuery() {
            string dateDiffFN = "DATEDIFF(day, physcntdate, GETDATE()) > inventory_.ccf";
            if (ApplicationConfiguration.IsOracle(DBType.Maximo)) {
                dateDiffFN = "(TRUNC(CURRENT_DATE) - trunc(physcntdate)) > inventory_.ccf";
            }
            return "RECONCILED = 1 and inventory_.ccf != 0 and {0} ".Fmt(dateDiffFN);
        }

    }
}
