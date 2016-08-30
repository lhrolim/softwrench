using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;

namespace softwrench.sW4.test.Util {
    public class TestContextLookuper : IContextLookuper {
        public void FillContext(ApplicationMetadataSchemaKey key) {
        }

        public void SetMemoryContext(string key, object ob, bool userSpecific = false) {
        }

        public void RemoveFromMemoryContext(string key, bool userSpecific = false) {
        }

        public T GetFromMemoryContext<T>(string key, bool userSpecific = false) {
            return default(T);
        }

        public ContextHolder LookupContext() {
            return null;
        }

        public void FillGridContext(string applicationName, InMemoryUser user) {
        }
    }
}
