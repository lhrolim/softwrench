using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace softwrench.sw4.Shared2.Metadata.Applications.Filter {
    public class SchemaFilters {
        public LinkedList<BaseMetadataFilter> Filters {
            get; private set;
        }

        public SchemaFilters(LinkedList<BaseMetadataFilter> filters) {
            Filters = filters;
        }

        public bool IsEmpty() {
            return Filters.Count == 0;
        }
    }
}
