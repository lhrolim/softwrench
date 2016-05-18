using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using cts.commons.portable.Util;
using cts.commons.Util;
using Newtonsoft.Json;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Rest;

namespace softWrench.sW4.Util {
    public static class ReflectionUtil {
        private const BindingFlags CommonBindingFlags = BindingFlags.Instance | BindingFlags.Public;

        public static Array InstantiateArray(Type arrayType, params object[] elements) {
            var length = elements == null ? 0 : elements.Length;
            var arr = Array.CreateInstance(arrayType, length);
            if (elements != null) {
                for (var i = 0; i < length; i++) {
                    arr.SetValue(elements.GetValue(i), i);
                }
            }
            return arr;
        }

        public static Array InstantiateArray(Type arrayType, int length) {
            var arr = Array.CreateInstance(arrayType, length);
            return arr;
        }

        public static object InstanceFromName(string typeName) {

            var type = Type.GetType(typeName);
            if (type != null) {
                return Activator.CreateInstance(type);
            }
            foreach (var asm in AssemblyLocator.GetSWAssemblies()) {
                type = asm.GetType(typeName);
                if (type != null) {
                    return Activator.CreateInstance(type);
                }
            }
            return null;

        }

        public static object InstanceFromType(Type type) {
            if (type.IsArray) {
                return InstantiateArray(type.GetElementType());
            }
            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// Returns a fresh instance of an object whose type is declared in baseobject.fieldName. This Object should have a 0 based constructor.
        /// </summary>
        /// <param name="baseObject"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public static object InstanceFromMember(object baseObject, String memberName) {
            if (baseObject == null) {
                return null;
            }
            var prop = TypeDescriptor.GetProperties(baseObject)[memberName];
            if (prop == null) {
                prop = TypeDescriptor.GetProperties(baseObject)[memberName.ToUpper()];
                if (prop == null) {
                    return null;
                }
            }
            var propertyType = prop.PropertyType;
            return InstanceFromType(propertyType);
        }


        /// <summary>
        /// Sets value to the property "propertyName" of the baseObject instance, returning whether the property was set or not.
        /// </summary>
        /// <param name="baseObject"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Boolean SetProperty(object baseObject, String propertyName, object value) {
            //search for the property name as is, fallbacking to the upper propertyname==> Some fields have "Value" as property while some have "VALUE"
            var prop = TypeDescriptor.GetProperties(baseObject)[propertyName] ??
                                      TypeDescriptor.GetProperties(baseObject)[propertyName.ToUpper()];
            if (prop == null) {
                return false;
            }
            try {
                if (value == null) {
                    prop.SetValue(baseObject, null);
                    return true;
                }
                var stvalue = value.ToString();
                if (prop.PropertyType.IsEnum) {
                    value = Enum.Parse(prop.PropertyType, stvalue);
                }
                //Add an other case for boolean
                if ("boolean".EqualsIc(prop.PropertyType.Name) && (!value.ToString().EqualsAny("true", "false"))) {
                    if (stvalue == "0") {
                        value = false;
                    }
                    if (stvalue == "1") {
                        value = true;
                    }
                }
                var castedValue = HandleCasting(prop, value);

                prop.SetValue(baseObject, castedValue);
            } catch (Exception e) {
                throw new InvalidOperationException(String.Format("Error setting property {0} of object {1}. {2}", propertyName, baseObject, e.Message), e);
            }
            return true;
        }

        private static object HandleCasting(PropertyDescriptor prop, object value) {
            if ("DateTime".EqualsIc(prop.PropertyType.Name) && !(value is DateTime)) {
                return ConversionUtil.HandleDateConversion(value as string);
            }

            return "Int64".EqualsIc(prop.PropertyType.Name) ? Convert.ToInt64(value) : value;
        }

        public static void SetProperty(object baseObject, dynamic innerPropertyValues) {
            foreach (var prop in innerPropertyValues.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                SetProperty(baseObject, prop.Name, prop.GetValue(innerPropertyValues, null));
            }
        }

        public static object InstantiateProperty(object baseObject, string propertyName, dynamic innerPropertyValues) {
            var newInstance = InstanceFromMember(baseObject, propertyName);
            foreach (var prop in innerPropertyValues.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                SetProperty(newInstance, prop.Name, prop.GetValue(innerPropertyValues, null));
            }
            SetProperty(baseObject, propertyName, newInstance);
            return newInstance;
        }

        public static object InstantiateProperty(object baseObject, string propertyName) {
            return InstantiateProperty(baseObject, propertyName, new { });
        }

        public static object InstantiateAndSetIfNull(object baseObject, string propertyName) {
            var value = GetProperty(baseObject, propertyName);
            if (value != null) {
                return value;
            }
            value = InstantiateProperty(baseObject, propertyName, new { });
            SetProperty(baseObject, propertyName, value);
            return value;
        }


        public static object InstantiateProperty(object baseObject, int index) {
            var prop = TypeDescriptor.GetProperties(baseObject)[index];
            var firstPropertyType = prop.PropertyType;
            var instance = InstanceFromType(firstPropertyType);
            prop.SetValue(baseObject, instance);
            return instance;

        }

        /// <summary>
        /// returns the Type of a property. If the property is a array, returns the type of the element in the array
        /// </summary>
        /// <param name="baseObject"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Type TypeOfProperty(object baseObject, string propertyName) {
            if (baseObject == null) {
                return null;
            }
            var prop = TypeDescriptor.GetProperties(baseObject)[propertyName];
            var type = prop.PropertyType;
            return type.HasElementType ? type.GetElementType() : type;
        }
        /// <summary>
        /// Instantiates an array of specified length  at the property propertyName of the baseObject, returning it
        /// </summary>
        /// <param name="baseObject"></param>
        /// <param name="propertyName"></param>
        /// <param name="arraySize"></param>
        /// <returns></returns>
        public static Array InstantiateArrayWithBlankElements(object baseObject, string propertyName, int arraySize) {
            if (baseObject == null) {
                return null;
            }
            var prop = TypeDescriptor.GetProperties(baseObject)[propertyName];
            var type = prop.PropertyType;
            if (!type.IsArray) {
                throw new ArgumentException(String.Format("property {0} is not an array", propertyName));
            }
            var elementType = type.GetElementType();
            var arr = InstantiateArray(elementType, arraySize);
            for (var i = 0; i < arraySize; i++) {
                var element = InstanceFromType(elementType);
                arr.SetValue(element, i);
            }
            prop.SetValue(baseObject, arr);
            return arr;
        }

        public static Array InstantiateArrayWithBlankElements(Type elementType, int arraySize) {
            var arr = InstantiateArray(elementType, arraySize);
            for (var i = 0; i < arraySize; i++) {
                var element = InstanceFromType(elementType);
                arr.SetValue(element, i);
            }
            return arr;
        }


        /// <summary>
        /// Instantiate an array in the baseObject.propertyName position, and put a single element on it, returning this element.
        /// </summary>
        /// <param name="baseObject"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object InstantiateArrayReturningSingleElement(object baseObject, string propertyName) {
            if (baseObject == null) {
                return null;
            }
            var prop = TypeDescriptor.GetProperties(baseObject)[propertyName];
            var type = prop.PropertyType;
            if (!type.IsArray) {
                throw new ArgumentException(String.Format("property {0} is not an array", propertyName));
            }
            var elementType = type.GetElementType();
            var element = InstanceFromType(elementType);
            var arr = InstantiateArray(elementType, element);
            prop.SetValue(baseObject, arr);
            return element;
        }

        /// <summary>
        /// Instantiate an element from an array in the baseObject.propertyName position, adding this element to it.
        /// </summary>
        /// <param name="baseObject"></param>
        /// <param name="propertyName"></param>
        /// <param name="markAsExclusive"></param>
        /// <returns></returns>
        public static object InstantiateSingleElementFromArray(object baseObject, string propertyName, bool markAsInline=false) {
            if (baseObject == null) {
                return null;
            }
            if (baseObject is IRestObjectWrapper) {
                var wrapper = (IRestObjectWrapper)baseObject;
                var restComposedData = new RestComposedData(markAsInline);
                wrapper.AddEntry(propertyName, restComposedData);
                return restComposedData.AddComposedData(null, 1, propertyName);
            }

            var prop = TypeDescriptor.GetProperties(baseObject)[propertyName];

            if (prop == null) {
                throw new MaximoException(string.Format("Array {0} is not declared in object {1}. Please contact support.", propertyName, baseObject.GetType()));
            }

            var type = prop.PropertyType;
            if (!type.IsArray) {
                throw new ArgumentException(string.Format("property {0} is not an array", propertyName));
            }
            var elementType = type.GetElementType();
            var element = InstanceFromType(elementType);

            var array = prop.GetValue(baseObject) as Array;

            if (array == null) {
                array = InstantiateArray(elementType, element);
                prop.SetValue(baseObject, array);
            } else {
                var newArray = Array.CreateInstance(elementType, array.Length + 1);
                Array.Copy(array, newArray, array.Length);
                newArray.SetValue(element, newArray.Length - 1);
                prop.SetValue(baseObject, newArray);
            }

            return element;
        }

        public static PropertyDescriptor GetPropertyDescriptor(object baseObject, string propertyName) {
            return BaseReflectionUtil.PropertyDescriptor(baseObject, propertyName);
        }

        public static object GetProperty(object baseObject, string propertyName) {
            var prop = BaseReflectionUtil.PropertyDescriptor(baseObject, propertyName);
            return prop == null ? null : prop.GetValue(baseObject);
        }

        public static object GetProperty(object baseObject, int index) {
            var prop = TypeDescriptor.GetProperties(baseObject)[index];
            return prop == null ? null : prop.GetValue(baseObject);
        }



        public static bool IsNull(object baseObject, string propertyName) {
            return ReflectionUtil.GetProperty(baseObject, propertyName) == null;
        }

        public static T DeepClone<T>(this T source) {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public static object Clone(object target, object source, params string[] propertyQualifier) {
            var targetProperties = TypeDescriptor.GetProperties(target);
            var sourceProperties = TypeDescriptor.GetProperties(source);
            foreach (PropertyDescriptor prop in targetProperties) {
                var sourceProperty = sourceProperties.Find(prop.Name, true);
                if (sourceProperty == null) continue;
                var value = sourceProperty.GetValue(source);
                if (value == null) continue;
                if (sourceProperty.PropertyType == prop.PropertyType) {
                    prop.SetValue(target, value);
                } else if (!sourceProperty.PropertyType.IsPrimitive && propertyQualifier != null) {
                    var o = prop.GetValue(target);
                    if (o == null) {
                        var newInstance = InstanceFromType(prop.PropertyType);
                        prop.SetValue(target, newInstance);
                        o = newInstance;
                    }
                    Clone(o, value);
                }
            }
            return target;
        }

        public static object Invoke(object baseObject, string method, object[] parameters, Type[] types = null) {
            var mi = types == null ? GetMethodNamed(baseObject, method) : baseObject.GetType().GetMethod(method, types);
            if (mi == null) {
                throw new InvalidOperationException(String.Format("No Method called {0} found in object {1}", method, baseObject.GetType().Name));
            }
            var retval = mi.Invoke(baseObject, CommonBindingFlags, null, parameters, null);
            return retval;
        }

        public static object InvokeWithNamedParameters(object baseObject, string method, IDictionary<string, object> namedParameters, Type[] types = null) {
            var mi = types == null ? GetMethodNamed(baseObject, method) : baseObject.GetType().GetMethod(method, types);
            if (mi == null) {
                throw new InvalidOperationException(String.Format("No Method called {0} found in object {1}", method, baseObject.GetType().Name));
            }
            var retval = mi.Invoke(baseObject, CommonBindingFlags, null, MapParameters(mi, namedParameters), null);
            return retval;
        }

        public static object[] MapParameters(MethodBase method, IDictionary<string, object> namedParameters) {
            var paramNames = method.GetParameters().Select(p => p.Name).ToArray();
            var parameters = new object[paramNames.Length];
            for (var i = 0; i < parameters.Length; ++i) {
                parameters[i] = Type.Missing;
            }
            foreach (var item in namedParameters) {
                if (item.Key.Contains(".")) {

                }
                var paramName = item.Key;
                var paramIndex = Array.IndexOf(paramNames, paramName);
                parameters[paramIndex] = item.Value;
            }
            return parameters;
        }


        public static MethodInfo GetMethodNamed(object baseObject, string operationName) {
            return baseObject.GetType().GetMethod(operationName);
        }


        public static T FindAttribute<T>(ApiController controller, string action, Type attributeType) where T : Attribute {
            var attr = GetMethodNamed(controller, action).GetCustomAttribute(attributeType);
            return attr == null ? (T)controller.GetType().GetCustomAttribute(attributeType) : (T)attr;
        }
    }
}
