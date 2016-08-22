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

        private static ICollection GetListOfAssemblies() {
            return ApplicationConfiguration.IsUnitTest 
                ? Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory)
                    .Where(file => Path.GetExtension(file) == ".dll")
                    .Select(Assembly.LoadFrom).ToList()
                : BuildManager.GetReferencedAssemblies();
        }

        public static bool IsSWAssembly(Assembly assembly) {
            var fullName = assembly.FullName;
            return fullName.StartsWith("softWrench", StringComparison.InvariantCultureIgnoreCase) || fullName.StartsWith("cts", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsCustomerAssembly(Assembly assembly) {
            return assembly.GetManifestResourceNames().Any(n => n.EndsWith("customer.placeholder"));
        }

        public static bool IsCurrentCustomerAssembly(Assembly assembly) {
            return assembly.FullName.StartsWith("softwrench.sw4.{0}".Fmt(ApplicationConfiguration.ClientName), StringComparison.CurrentCultureIgnoreCase);
        }

        public static IEnumerable<Assembly> GetSWAssemblies() {
            return GetAssemblies().Where(a => {
                var isSw = IsSWAssembly(a);
                var isCustomer = IsCustomerAssembly(a);
                var isCurrentCustomer = IsCurrentCustomerAssembly(a);
                return (isSw && !isCustomer) || (isSw && isCurrentCustomer);
            });
        }

        public static bool CustomerAssemblyExists() {
            return GetAssemblies().Any(IsCurrentCustomerAssembly);
        }

        public static Assembly GetCustomerAssembly() {
            return GetAssemblies().FirstOrDefault(IsCurrentCustomerAssembly);
        }

        public static Assembly GetAssembly(string assemblyName) {
            return GetAssemblies().FirstOrDefault(r => r.FullName.StartsWith(assemblyName, StringComparison.CurrentCultureIgnoreCase));
        }

        public static IEnumerable<Assembly> GetMigratorAssemblies() {
            return GetSWAssemblies();
        }
    }
}
