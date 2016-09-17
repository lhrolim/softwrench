using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SimpleInjector.Advanced;

namespace cts.commons.simpleinjector {
    /// <summary>
    /// taken from http://simpleinjector.readthedocs.io/en/latest/advanced.html#property-injection
    /// </summary>
    public class ImportPropertySelectionBehavior : IPropertySelectionBehavior {

        public bool SelectProperty(Type type, PropertyInfo prop) {
            return prop.GetCustomAttributes(typeof(ImportAttribute)).Any();
        }
    }
}
