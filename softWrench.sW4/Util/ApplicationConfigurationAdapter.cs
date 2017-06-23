using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;

namespace softWrench.sW4.Util {
    public class ApplicationConfigurationAdapter : IApplicationConfiguration, ISingletonComponent {
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

        public bool IsProd() {
            return ApplicationConfiguration.IsProd();
        }

        public bool IsDev() {
            return ApplicationConfiguration.IsDev();
        }

        public bool IsQa() {
            return ApplicationConfiguration.IsQA();
        }

        public bool IsUat() {
            return ApplicationConfiguration.IsUat();
        }

        public bool IsLocal() {
            return ApplicationConfiguration.IsLocal();
        }

        public bool IsUnitTest {
            get {
                return ApplicationConfiguration.IsUnitTest;
            }
        }
    }
}
