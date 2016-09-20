using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using SimpleInjector;

namespace cts.commons.simpleinjector {
    public class SimpleInjectoScannerUtil {

        private static readonly ILog Log = LogManager.GetLogger(typeof(SimpleInjectoScannerUtil));

        public static void RegisterOverridingBaseClass(Container container, string clientName, OverridingComponentAttribute overridingAnnotation, Type realType, Registration simpleInjectorRegistration, string name) {
            var type = realType.BaseType;
            if (overridingAnnotation.ClientFilters != null && (!overridingAnnotation.ClientFilters.Split(',').Contains(clientName))) {
                Log.DebugFormat("ignoring overriding type {0} due to client filters", realType.Name);
                return;
            }

            if (type == null || (!type.IsPublic && !type.IsNestedPublic)) {
                return;
            }

            Log.DebugFormat("registering replacement {0} for base class {1}", realType.Name, type.Name);
            container.AddRegistration(type, simpleInjectorRegistration);
            SimpleInjectorGenericFactory.RegisterNameAndType(realType, name);
        }

        public static void RegisterFromAttribute(ComponentAttribute attr, IDictionary<Type, IList<Registration>> tempDict, Registration registration) {
            var registrationType = attr.RegistrationType;
            if (registrationType == null) {
                return;
            }
            if (!tempDict.ContainsKey(registrationType)) {
                tempDict.Add(registrationType, new List<Registration>());
            }
            tempDict[registrationType].Add(registration);
        }
    }
}
