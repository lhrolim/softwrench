using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.api.classes.application {

    public abstract class SchemaBasedApplicationFiltereable : IGenericApplicationFiltereable {

        public abstract string ApplicationName();
        public abstract string ClientFilter();
        public string ExtraKey() {
            return SchemaId();
        }

        public abstract string SchemaId();
        
    }
}
