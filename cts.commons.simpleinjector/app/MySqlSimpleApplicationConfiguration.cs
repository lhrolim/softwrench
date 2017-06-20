using cts.commons.persistence;

namespace cts.commons.simpleinjector.app {
    [IgnoreComponent]
    class MySqlSimpleApplicationConfiguration : IApplicationConfiguration {
        public bool IsDB2(DBType maximo) {
            return false;
        }

        public DBMS? LookupDBMS(DBType dbtype) {
            return DBMS.MYSQL;
        }

        public bool IsOracle(DBType maximo) {
            return false;
        }

        public virtual string GetClientKey() {
            return "";
        }

        public bool IsProd() {
            return false;
        }

        public bool IsDev() {
            return false;
        }

        public bool IsQa() {
            return false;
        }

        public bool IsUat() {
            return false;
        }

        public bool IsLocal() {
            return false;
        }

        public bool IsUnitTest {
            get {
                return false;
            }
        }
    }
}
