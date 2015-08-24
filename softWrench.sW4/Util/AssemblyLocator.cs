using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using cts.commons.portable.Util;

namespace softWrench.sW4.Util {
    public static class AssemblyLocator {
        private static ReadOnlyCollection<Assembly> AllAssemblies;


        public static ReadOnlyCollection<Assembly> GetAssemblies() {
            if (AllAssemblies != null) {
                return AllAssemblies;
            }

            AllAssemblies = new ReadOnlyCollection<Assembly>(
              GetListOfAssemblies().Cast<Assembly>().ToList());

            return AllAssemblies;
        }

        private static ICollection GetListOfAssemblies()
        {
            if (ApplicationConfiguration.IsUnitTest) {
                var enumerable = Directory.GetFiles(
                    AppDomain.CurrentDomain.BaseDirectory)
                    .Where(file => Path.GetExtension(file) == ".dll");
                return enumerable
                    .Select(file => Assembly.LoadFrom(file)).ToList();
            }
            return BuildManager.GetReferencedAssemblies();
        }

        public static IEnumerable<Assembly> GetSWAssemblies() {
            return
                GetAssemblies()
                    .Where(r => r.FullName.StartsWith("softWrench", StringComparison.InvariantCultureIgnoreCase) || r.FullName.StartsWith("cts", StringComparison.InvariantCultureIgnoreCase));
        }

       

        public static bool CustomerAssemblyExists() {
            return GetAssemblies().Any(r => r.FullName.StartsWith("softwrench.sw4.{0}".Fmt(ApplicationConfiguration.ClientName), StringComparison.CurrentCultureIgnoreCase));
        }

        public static Assembly GetCustomerAssembly() {
            return GetAssemblies().FirstOrDefault(r => r.FullName.StartsWith("softwrench.sw4.{0}".Fmt(ApplicationConfiguration.ClientName), StringComparison.CurrentCultureIgnoreCase));
        }

        public static Assembly GetAssembly(string assemblyName) {
            return GetAssemblies().FirstOrDefault(r => r.FullName.StartsWith(assemblyName, StringComparison.CurrentCultureIgnoreCase));
        }

        public static IEnumerable<Assembly> GetMigratorAssemblies() {
            return GetSWAssemblies();
        }
    }
}
