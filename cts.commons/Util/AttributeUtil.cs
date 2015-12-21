using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace softWrench.sW4.Util {

    /// <summary>
    /// Utility class for methods related with attributes (Annotations in Java World)
    /// </summary>
    public static class AttributeUtil {


        private static readonly IDictionary<Type, ISet<Type>> AttributesCache
            = new Dictionary<Type, ISet<Type>>();

        /// <summary>
        /// Lookups for the given annotation in all of the softwrench dlls (hence, excluding third-party useless searchs) of the executing program.
        /// 
        /// Does not look for inherited classes, only the exact match, as this is usual the desired behaviour
        /// 
        /// </summary>
        /// <param name="typeToSearch"></param>
        /// <returns></returns>
        public static ISet<Type> FindTypesAnnotattedWith(IEnumerable<Assembly> assembliesToSearch,Type typeToSearch) {
            if (AttributesCache.ContainsKey(typeToSearch)) {
                return AttributesCache[typeToSearch];
            }
            var result = FindTypesAnnotattedWith(assembliesToSearch,new[] { typeToSearch });
            AttributesCache[typeToSearch] = result;
            return result;
        }


        //TODO:cache this call
        public static ISet<Type> FindTypesAnnotattedWith(IEnumerable<Assembly> assembliesToSearch,params Type[] typesToSearch) {
            var resulTypes = new HashSet<Type>();
            IList<Type> typesNotYetCached = new List<Type>();
            foreach (var typeToSearch in typesToSearch) {
                if (!AttributesCache.ContainsKey(typeToSearch)) {
                    typesNotYetCached.Add(typeToSearch);
                    AttributesCache[typeToSearch] = new HashSet<Type>();
                } else {
                    resulTypes.AddAll(AttributesCache[typeToSearch]);
                }
            }
            if (!typesNotYetCached.Any()) {
                return resulTypes;
            }

            var swAssemblies = assembliesToSearch;
            foreach (var swAssembly in swAssemblies) {
                foreach (var type in swAssembly.GetTypes()) {
                    foreach (var typeToSearch in typesNotYetCached) {
                        if (type.GetCustomAttributes(typeToSearch, false).Length > 0) {
                            AttributesCache[typeToSearch].Add(type);
                            resulTypes.Add(type);
                        }
                    }
                }
            }
            return resulTypes;
        }


        public static IList<PropertyInfo> FindPropertiesWithAttribute(this Type type, Type attributeType) {

            IList<PropertyInfo> result = new List<PropertyInfo>();
            var propertyInfos = type.GetProperties();
            foreach (var propertyInfo in propertyInfos) {
                var attr = propertyInfo.GetCustomAttribute(attributeType);
                if (attr != null) {
                    result.Add(propertyInfo);
                }
            }
            return result;
        }


        /// <summary>
        /// Returns the value of a member attribute for any member in a class.
        ///     (a member is a Field, Property, Method, etc...)    
        /// <remarks>
        /// If there is more than one member of the same name in the class, it will return the first one (this applies to overloaded methods)
        /// </remarks>
        /// <example>
        /// Read System.ComponentModel Description Attribute from method 'MyMethodName' in class 'MyClass': 
        ///     var Attribute = typeof(MyClass).GetAttribute("MyMethodName", (DescriptionAttribute d) => d.Description);
        /// </example>
        /// <param name="type">The class that contains the member as a type</param>
        /// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. This parameter is ignored for properties and events</param>
        /// </summary>    
        public static TAttribute ReadAttribute<TAttribute>(this Type type, bool inherit = false) where TAttribute : Attribute {
            var att = type.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault() as TAttribute;
            if (att != null) {
                return att;
            }
            return default(TAttribute);
        }

        public static TAttribute ReadAttribute<TAttribute>(this MemberInfo type, bool inherit = false) where TAttribute : Attribute {
            var att = type.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault() as TAttribute;
            if (att != null) {
                return att;
            }
            return default(TAttribute);
        }



        /// <summary>
        /// Returns the value of a member attribute for any member in a class.
        ///     (a member is a Field, Property, Method, etc...)    
        /// <remarks>
        /// If there is more than one member of the same name in the class, it will return the first one (this applies to overloaded methods)
        /// </remarks>
        /// <example>
        /// Read System.ComponentModel Description Attribute from method 'MyMethodName' in class 'MyClass': 
        ///     var Attribute = typeof(MyClass).GetAttribute("MyMethodName", (DescriptionAttribute d) => d.Description);
        /// </example>
        /// <param name="type">The class that contains the member as a type</param>
        /// <param name="memberName">Name of the member in the class</param>
        /// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. This parameter is ignored for properties and events</param>
        /// </summary>    
        public static TAttribute GetAttributeOfMember<TAttribute>(this Type type, string memberName, bool inherit = false) where TAttribute : Attribute {

            var member = type.GetMember(memberName).FirstOrDefault();
            if (member == null) {
                return default(TAttribute);
            }

            var att = member.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault() as TAttribute;
            if (att != null) {
                return att;
            }
            return default(TAttribute);
        }

    }
}
