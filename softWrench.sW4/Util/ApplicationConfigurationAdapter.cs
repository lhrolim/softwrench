using cts.commons.persistence;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;

namespace softWrench.sW4.Util {
    public class ApplicationConfigurationAdapter : IApplicationConfiguration {
        public bool IsDB2(DBType maximo) {
            return ApplicationConfiguration.IsDB2(maximo);
        }

        public DBMS? LookupDBMS(DBType dbtype) {
            return ApplicationConfiguration.ToUse(dbtype);
        }

        public bool IsOracle(DBType maximo) {
            return ApplicationConfiguration.IsOracle(maximo);
        }

        public string GetClientKey() {
            return ApplicationConfiguration.ClientName;
        }

        public bool IsUnitTest {
            get {
                return ApplicationConfiguration.IsUnitTest;
            }
        }
    }
}
