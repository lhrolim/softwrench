using cts.commons.persistence;

namespace cts.commons.simpleinjector.app {
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

        public bool IsUnitTest {
            get { return false; }
        }
    }
}
