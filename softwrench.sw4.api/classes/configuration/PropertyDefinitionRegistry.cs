using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.api.classes.configuration {

    public class PropertyDefinitionRegistry {

        public string Description { get; set; }
        public string DataType { get; set; }
        public string DefaultValue { get; set; }
        public bool CachedOnClient { get; set; }
    }
}
