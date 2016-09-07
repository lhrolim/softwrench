using System;
using System.Collections.Generic;
using SimpleInjector;

namespace cts.commons.simpleinjector {

    public class SimpleInjectorGenericFactory : ISingletonComponent {

        private readonly Container _container;

        public static SimpleInjectorGenericFactory Instance = null;

        private static readonly IDictionary<string, Type> StringTypeDictionary = new Dictionary<string, Type>();
        private static readonly IDictionary<string, Type> DynOriginalTypeDictionary = new Dictionary<string, Type>();

        private static readonly IDictionary<Type, ISingletonComponent> SingletonCache = new Dictionary<Type, ISingletonComponent>();

        public SimpleInjectorGenericFactory(Container container) {
            this._container = container;
            Instance = this;

        }

        /// <summary>
        /// Do not cache the result of this method, the singletons are already cached. If you do so a memory leak can occur.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public T GetObject<T>(Type type = null) {
            try {
                type = type ?? typeof(T);
                if (!typeof(ISingletonComponent).IsAssignableFrom(type)) {
                    // Return job registrated in container
                    return (T)_container.GetInstance(type);
                }
                if (!SingletonCache.ContainsKey(type)) {
                    SingletonCache[type] = (ISingletonComponent)_container.GetInstance(type);
                }
                return (T)SingletonCache[type];
            } catch (Exception ex) {
                throw new Exception("Problem instantiating class", ex);
            }
        }

        public IEnumerable<T> GetObjectsOfType<T>(Type type) {
            try {
                return _container.GetAllInstances<T>();
            } catch (Exception ex) {
                throw new Exception("Problem instantiating class", ex);
            }
        }

        /// <summary>
        /// Do not cache the result of this method, the singletons are already cached. If you do so a memory leak can occur.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public T GetObject<T>(string serviceName) {
            try {
                // Return job registrated in container
                return StringTypeDictionary.ContainsKey(serviceName) ? GetObject<T>(StringTypeDictionary[serviceName]) : default(T);
            } catch (Exception ex) {
                throw new Exception("Problem instantiating class", ex);
            }
        }

        public static bool ContainsService(string serviceName) {
            return StringTypeDictionary.ContainsKey(serviceName);
        }

        public static string BuildRegisterName(Type type) {
            var attribute = (ComponentAttribute)Attribute.GetCustomAttribute(type, typeof(ComponentAttribute));
            var ovrattribute = (OverridingComponentAttribute)Attribute.GetCustomAttribute(type, typeof(OverridingComponentAttribute));

            var realTypeAttr = type;
            if (ovrattribute != null && type.BaseType != null) {
                realTypeAttr = type.BaseType;
            }

            var name = char.ToLowerInvariant(realTypeAttr.Name[0]) + realTypeAttr.Name.Substring(1);
            if (attribute != null && !string.IsNullOrEmpty(attribute.Name)) {
                name = attribute.Name;
            }
            return name;
        }

        public static void RegisterNameAndType(Type type, string preName = null) {
            var ovrattribute = (OverridingComponentAttribute)Attribute.GetCustomAttribute(type, typeof(OverridingComponentAttribute));
            var overriding = ovrattribute != null && type.BaseType != null;

            var name = preName ?? BuildRegisterName(type);
            if (!StringTypeDictionary.ContainsKey(name)) {
                StringTypeDictionary[name] = type;
            } else if (overriding) {
                StringTypeDictionary.Remove(name);
                StringTypeDictionary[name] = type;
            }
        }

        public static void RegisterDynOriginalType(Type type, string name) {
            DynOriginalTypeDictionary[name] = type;
        }

        public static Type GetDynOriginalType(string name) {
            return !DynOriginalTypeDictionary.ContainsKey(name) ? null : DynOriginalTypeDictionary[name];
        }

        public static void CacheSingletons() {
            Instance._container.GetAllInstances<ISingletonComponent>();
        }

        public static void ClearCache() {
            StringTypeDictionary.Clear();
            DynOriginalTypeDictionary.Clear();
            SingletonCache.Clear();
        }
    }
}
