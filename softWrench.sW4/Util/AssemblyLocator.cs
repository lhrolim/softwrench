using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using cts.commons.portable.Util;
using cts.commons.Util;

namespace softWrench.sW4.Util {
    public static class AssemblyLocator {
        private static ReadOnlyCollection<Assembly> AllAssemblies;
        private static ReadOnlyCollection<Assembly> BinAssemblies;
    

        public static ReadOnlyCollection<Assembly> GetAssemblies() {
            if (AllAssemblies != null)
            {
                return AllAssemblies;
            }

            AllAssemblies = new ReadOnlyCollection<Assembly>(
              BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToList());

            IList<Assembly> binAssemblies = new List<Assembly>();

            string binFolder = HttpRuntime.AppDomainAppPath + "bin\\";
            IList<string> dllFiles = Directory.GetFiles(binFolder, "*.dll",
                SearchOption.TopDirectoryOnly).ToList();

            foreach (string dllFile in dllFiles) {
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(dllFile);

                Assembly locatedAssembly = AllAssemblies.FirstOrDefault(a =>
                    AssemblyName.ReferenceMatchesDefinition(
                        a.GetName(), assemblyName));

                if (locatedAssembly != null) {
                    binAssemblies.Add(locatedAssembly);
                }
            }

            BinAssemblies = new ReadOnlyCollection<Assembly>(binAssemblies);
            return AllAssemblies;
        }

        public static IEnumerable<Assembly> GetSWAssemblies() {
            return
                GetAssemblies()
                    .Where(r => r.FullName.StartsWith("softWrench", StringComparison.InvariantCultureIgnoreCase) || r.FullName.StartsWith("cts", StringComparison.InvariantCultureIgnoreCase));
        }

        public static ReadOnlyCollection<Assembly> GetBinFolderAssemblies() {
            return BinAssemblies;
        }

        public static bool CustomerAssemblyExists() {
            return GetAssemblies().Any(r => r.FullName.StartsWith("softwrench.sw4.{0}".Fmt(ApplicationConfiguration.ClientName), StringComparison.CurrentCultureIgnoreCase));
        }

        public static Assembly GetCustomerAssembly()
        {
            return GetAssemblies().FirstOrDefault(r => r.FullName.StartsWith("softwrench.sw4.{0}".Fmt(ApplicationConfiguration.ClientName), StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
