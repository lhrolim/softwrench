using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iesi.Collections.Generic;

namespace softWrench.sW4.Util {

    /// <summary>
    /// Utility class for methods related with attributes (Annotations in Java World)
    /// </summary>
    public class AttributeUtil {

        /// <summary>
        /// Lookups for the given annotation in all of the softwrench dlls (hence, excluding third-party useless searchs) of the executing program.
        /// 
        /// Does not look for inherited classes, only the exact match, as this is usual the desired behaviour
        /// 
        /// </summary>
        /// <param name="typeToSearch"></param>
        /// <returns></returns>
        public static ISet<Type> FindTypesAnnotattedWith(Type typeToSearch) {
            return FindTypesAnnotattedWith(new Type[] { typeToSearch });
        }

        public static ISet<Type> FindTypesAnnotattedWith(params Type[] typesToSearch) {
            ISet<Type> resulTypes = new HashedSet<Type>();

            var swAssemblies = AssemblyLocator.GetSWAssemblies();
            foreach (var swAssembly in swAssemblies) {
                foreach (var type in swAssembly.GetTypes()) {
                    foreach (var typeToSearch in typesToSearch) {
                        if (type.GetCustomAttributes(typeToSearch, false).Length > 0) {
                            resulTypes.Add(type);
                        }
                    }
                }
            }
            return resulTypes;
        }
    }
}
