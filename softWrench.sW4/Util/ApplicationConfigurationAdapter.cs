using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
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
    }
}
