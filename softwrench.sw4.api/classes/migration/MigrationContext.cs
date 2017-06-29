using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.api.classes.migration {

    public class MigrationContext {

        public static string ServerType { get; set; }

        public static bool IsOracle {
            get {
                return "oracle".Equals(ServerType, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public static bool IsDb2 {
            get {
                return "db2".Equals(ServerType, StringComparison.CurrentCultureIgnoreCase);
            }
        }


    }
}
