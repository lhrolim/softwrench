using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sW4.Shared2.Metadata;

namespace softwrench.sw4.Shared2.Metadata {
    /// <summary>    
    /// <see cref="softwrench.sW4.Shared2.Metadata.IDefinition" />
    /// OBS: this class is used only for mobile project
    /// </summary>
    public abstract class BaseDefinition : IDefinition {

        public IDictionary<string, object> ExtensionParameters { get; set; }

        public object ExtensionParameter(string key) {
            object obj;
            if (ExtensionParameters == null) {
                ExtensionParameters = new Dictionary<string, object>();
            }
            ExtensionParameters.TryGetValue(key, out obj);
            return obj;
        }

        public void ExtensionParameter(string key, object value) {
            if (ExtensionParameters == null) {
                ExtensionParameters = new Dictionary<string, object>();
            }
            ExtensionParameters.Add(key, value);
        }

        
    }
}
