using System;
using System.Collections.Generic;
using cts.commons.portable.Util;
using SimpleInjector;

namespace cts.commons.simpleinjector {

    public class SimpleInjectorGenericFactory : ISingletonComponent {

        private readonly Container _container;

        public static SimpleInjectorGenericFactory Instance = null;

        private static readonly IDictionary<string, Type> StringTypeDictionary = new Dictionary<string, Type>();

        public SimpleInjectorGenericFactory(Container container) {
            this._container = container;
            Instance = this;
        }

        public T GetObject<T>(Type type) {
            try {
                // Return job registrated in container
                return (T)_container.GetInstance(type);
            } catch (Exception ex) {
                throw new Exception(
                    "Problem instantiating class", ex);
            }
        }

        public IEnumerable<T> GetObjectsOfType<T>(Type type) {
            try {
                return _container.GetAllInstances<T>();
            } catch (Exception ex) {
                throw new Exception(
                    "Problem instantiating class", ex);
            }
        }

        public T GetObject<T>(string serviceName) {
            try {
                // Return job registrated in container
                if (StringTypeDictionary.ContainsKey(serviceName)) {
                    return (T)_container.GetInstance(StringTypeDictionary[serviceName]);
                }
                return default(T);
            } catch (Exception ex) {
                throw new Exception(
                    "Problem instantiating class", ex);
            }
        }

        public static bool ContainsService(string serviceName, string methodName) {
            return StringTypeDictionary.ContainsKey(serviceName);
        }


        public static bool ContainsService(Type type) {
            var name = char.ToLowerInvariant(type.Name[0]) + type.Name.Substring(1);
            return ContainsService(name, null);
        }


        public static void RegisterNameAndType(Type type) {
            var attribute = (ComponentAttribute)Attribute.GetCustomAttribute(type, typeof(ComponentAttribute));

            var ovrattribute = (OverridingComponentAttribute)Attribute.GetCustomAttribute(type, typeof(OverridingComponentAttribute));

            var realTypeAttr = type;
            var overriding = ovrattribute != null && type.BaseType != null;

            if (ovrattribute != null && type.BaseType != null) {
                realTypeAttr = type.BaseType;
            }


            var name = char.ToLowerInvariant(realTypeAttr.Name[0]) + realTypeAttr.Name.Substring(1);

            if (attribute != null && !string.IsNullOrEmpty(attribute.Name)) {
                name = attribute.Name;
            }
            if (!StringTypeDictionary.ContainsKey(name)) {
                StringTypeDictionary[name] = type;
            } else if (overriding) {
                StringTypeDictionary.Remove(name);
                StringTypeDictionary[name] = type;
            }
        }


    }
}
