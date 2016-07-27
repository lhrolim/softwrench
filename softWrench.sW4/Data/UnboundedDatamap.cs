using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace softWrench.sW4.Data {
    public class UnboundedDatamap : DataMap {
        public UnboundedDatamap([NotNull] string application, [NotNull] IDictionary<string, object> fields, Type mappingType = null, bool rowstampsHandled = false) : base(application, fields, mappingType, rowstampsHandled) {
        }

        public UnboundedDatamap([NotNull] string application, [NotNull] IDictionary<string, object> fields, string idFieldName) : base(application, fields, idFieldName) {
        }

        public string Type {get {return GetType().Name;}
        }


    }
}
