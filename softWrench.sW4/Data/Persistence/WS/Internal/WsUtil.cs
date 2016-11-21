using log4net;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal.Constants;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using cts.commons.Util;
using softWrench.sW4.Data.Persistence.WS.Rest;
using softWrench.sW4.Util.DeployValidation;
using r = softWrench.sW4.Util.ReflectionUtil;

namespace softWrench.sW4.Data.Persistence.WS.Internal {
    public class WsUtil {
        //        private const String MEA_ENTITIES_PACKAGE = "softWrench.sW4.Data.Persistence.WS.Mea.Entities";
        //        private const String MIF_ENTITIES_PACKAGE = "softWrench.sW4.Data.Persistence.WS.Mif.Entities";

        private static readonly ILog Log = LogManager.GetLogger(typeof(WsUtil));

        public static WsProvider WsProvider() {
            var target = MetadataProvider.TargetMapping();
            var prop = MetadataProvider.GlobalProperty("wsprovider", true);
            
            if (prop != null) {
                WsProvider val;
                Enum.TryParse(prop, true, out val);
                return val;
            }

            if (target == "maximo7.5" || target == "maximo7.1" || target == "maximo7.2.1" || target == "smartcloud7.5") {
                return Constants.WsProvider.MIF;
            }
            if (target == "rest") {
                return Constants.WsProvider.REST;
            }

            if (target == "ism") {
                return Constants.WsProvider.ISM;
            }
            if (target.StartsWith("maximo6")) {
                return Constants.WsProvider.MEA;
            }

            return Constants.WsProvider.MIF;
        }

        public static bool Is75OrNewer() {
            var target = MetadataProvider.TargetMapping();
            return target.Contains("7.5") || target.Contains("7.6");
        }

        public static bool Is71() {
            var target = MetadataProvider.TargetMapping();
            return target.Contains("7.1");
        }

        public static object GetRealValue(object integrationObject, string propertyName) {
            if (integrationObject is IRestObjectWrapper) {
                //rest adapter
                var dict = ((IRestObjectWrapper)integrationObject).Entries;
                if (dict.ContainsKey(propertyName)) {
                    return dict[propertyName];
                }
                return null;
            }


            var property = ReflectionUtil.GetProperty(integrationObject, propertyName);
            if (property == null) {
                return null;
            }
            var value = ReflectionUtil.GetProperty(property, "Value");
            return value;
        }

        public static T GetRealValue<T>(object integrationObject, string propertyName) {
            if (integrationObject is IRestObjectWrapper) {
                //rest adapter
                var dict = ((IRestObjectWrapper)integrationObject).Entries;
                if (dict.ContainsKey(propertyName)) {
                    return (T)dict[propertyName];
                }
                return default(T);
            }

            var property = ReflectionUtil.GetProperty(integrationObject, propertyName);
            if (property == null) {
                return default(T);
            }
            var value = ReflectionUtil.GetProperty(property, "Value");
            return (T)value;
        }



        public static void SetChanged(object baseObjectWithAction) {
            ReflectionUtil.SetProperty(baseObjectWithAction, "changed", true);
            ReflectionUtil.SetProperty(baseObjectWithAction, "changedSpecified", true);
        }

        public static void SetChanged(params object[] baseObjectWithAction) {
            foreach (var ob in baseObjectWithAction) {
                SetChanged(ob);
            }

        }

        public static object SetValueIfNull(object baseObject, string propertyName, object value,
            bool markSpecified = false, bool setIfNegativeToo = false) {
            object currentValue;
            if (baseObject is IRestObjectWrapper) {
                //rest adapter
                var dict = ((IRestObjectWrapper)baseObject).Entries;
                if (!dict.ContainsKey(propertyName)) {
                    dict[propertyName] = value;
                    return value;
                }
                currentValue = dict[propertyName];
                if ((setIfNegativeToo && Convert.ToInt64(currentValue) < 0)) {
                    dict[propertyName] = value;
                    return value;
                }
            }

            if (baseObject == null) {
                Log.Warn(string.Format("property {0} not found on object null", propertyName));
                return null;
            }
            var property = ReflectionUtil.GetPropertyDescriptor(baseObject, propertyName);
            if (property == null) {
                DeployValidationService.AddMissingProperty(propertyName);
                Log.Warn(string.Format("property {0} not found on object {1}", propertyName, baseObject.GetType()));
                return null;
            }
            var propertyVal = property.GetValue(baseObject);
            currentValue = ReflectionUtil.GetProperty(propertyVal, "Value");
            if (currentValue == null || (setIfNegativeToo && Convert.ToInt64(currentValue) < 0)) {
                return SetValue(baseObject, propertyName, value, markSpecified);
            }
            return property;
        }


        public static void CopyFromRootEntity(object rootObject, object integrationObject, string propertyName, object defaultValue, string rootPropertyName = null, bool onlyIfNull = true) {
            var rootPropertyNameToUse = rootPropertyName ?? propertyName;
            var currentValue = GetRealValue(integrationObject, propertyName);
            if (onlyIfNull && currentValue != null) {
                return;
            }
            var rootValue = GetRealValue(rootObject, rootPropertyNameToUse);
            var valuetoUse = rootValue ?? defaultValue;
            SetValue(integrationObject, propertyName, valuetoUse);
        }

        public static object SetValue(object baseObject, string propertyName, object value, bool markSpecified = false) {
            if (baseObject is IRestObjectWrapper) {
                //rest adapter
                var dict = ((IRestObjectWrapper)baseObject).Entries;
                dict[propertyName] = value;
                return value;
            }

            var propDescriptor = BaseReflectionUtil.PropertyDescriptor(baseObject, propertyName);
            if (propDescriptor == null) {
                DeployValidationService.AddMissingProperty(propertyName);
                if (ApplicationConfiguration.IsLocal()) {
                    Log.WarnFormat("property {0} not found on object {1}. Review metadata config or maximo config",
                        propertyName, baseObject);
                }
                return null;
            }
            var propertyType = propDescriptor.PropertyType;
            var isPrimitive = propertyType.IsPrimitive || typeof(DateTime).IsAssignableFrom(propertyType) || propertyType == typeof(string);
            if (isPrimitive) {
                propDescriptor.SetValue(baseObject, value);
                //if (markSpecified) {
                //    SetValue(baseObject, propertyName + "Specified", true);
                //}
                return value;
            }
            var prop = propDescriptor.GetValue(baseObject);
            if (prop == null) {
                return ReflectionUtil.InstantiateProperty(baseObject, propertyName, new { Value = value });
            }
            ReflectionUtil.SetProperty(prop, new { Value = value });
            //if (markSpecified) {
            //    SetValue(baseObject, propertyName + "Specified", true);
            //}
            return prop;
        }



        public static object SetQueryValue(object baseObject, string propertyName, object value, QueryOperator operatorType = QueryOperator.Equals) {
            var queryField = SetValue(baseObject, propertyName, value);
            ReflectionUtil.SetProperty(queryField, "@operator", operatorType.MaximoValue());
            ReflectionUtil.SetProperty(queryField, "operatorSpecified", true);
            return queryField;
        }


        public static void SetChangeBy(object baseObject) {
            var user = SecurityFacade.CurrentUser();
            ReflectionUtil.InstantiateProperty(baseObject, "CHANGEDATE", new { Value = DateTime.Now.FromServerToRightKind() });
            ReflectionUtil.InstantiateProperty(baseObject, "CHANGEBY", new { Value = user.Login });
        }

        public static object CloneExistingProperties(object target, object source) {
            return ReflectionUtil.Clone(target, source, "value");
        }

        public static object CloneFromEntity(object target, AttributeHolder attributes) {
            var targetProperties = TypeDescriptor.GetProperties(target);
            foreach (PropertyDescriptor prop in targetProperties) {
                var value = attributes.GetAttribute(prop.Name.ToLower());
                if (value == null) continue;
                if (prop.PropertyType.IsPrimitive) {
                    prop.SetValue(target, value);
                } else if (!prop.PropertyType.IsPrimitive) {
                    var o = prop.GetValue(target);
                    if (o == null) {
                        var newInstance = ReflectionUtil.InstanceFromType(prop.PropertyType);
                        prop.SetValue(target, newInstance);
                        o = newInstance;
                    }
                    ReflectionUtil.SetProperty(o, new { Value = value });
                }
            }
            return target;
        }

        public static void NullifyValue(object baseObject, string propertyName) {
            if (baseObject is IRestObjectWrapper) {
                //rest adapter
                var dict = ((IRestObjectWrapper)baseObject).Entries;
                dict.Remove(propertyName);
            } else {
                ReflectionUtil.SetProperty(baseObject, propertyName, null);
            }
        }

        public static void CloneArray(IEnumerable<CrudOperationData> collectionAssociation, object integrationObject, string propertyName, Action<object, CrudOperationData> itemHandlerDelegate = null) {
            // TODO: review the #isDirty strategy
            // apply cloning only to dirty items --> ignoring unchanged data
            // var crudOperationDatas = collectionAssociation.Where(data => data.UnmappedAttributes.ContainsKey("#isDirty")).ToList();

            var crudOperationDatas = collectionAssociation as IList<CrudOperationData> ?? collectionAssociation.ToList();

            if (!crudOperationDatas.Any()) {
                return;
            }

            if (integrationObject is IRestObjectWrapper) {
                //rest adapter
                var wrapper = ((IRestObjectWrapper)integrationObject);
                var dict = wrapper.Entries;
                var composedData = new RestComposedData();
                dict[propertyName] = composedData;

                for (var i = 0; i < crudOperationDatas.Count(); i++) {
                    var crudOperationData = crudOperationDatas[i];
                    var cloneFromEntity = composedData.AddComposedData(crudOperationData, propertyName);
                    if (itemHandlerDelegate != null) {
                        itemHandlerDelegate(cloneFromEntity, crudOperationData);
                    }
                }
            } else {
                var arr = ReflectionUtil.InstantiateArrayWithBlankElements(integrationObject, propertyName, crudOperationDatas.Count);
                for (var i = 0; i < arr.Length; i++) {
                    var crudOperationData = crudOperationDatas[i];
                    var cloneFromEntity = WsUtil.CloneFromEntity(arr.GetValue(i), crudOperationData);
                    if (itemHandlerDelegate != null) {
                        itemHandlerDelegate(cloneFromEntity, crudOperationData);
                    }
                    //                WsUtil.SetValue(cloneFromEntity, "worklogid", -1);
                    arr.SetValue(cloneFromEntity, i);
                }
            }
        }

        public static void CloneSingle(CrudOperationData crudOperationData, object integrationObject, string propertyName, Action<object, CrudOperationData> itemHandlerDelegate = null) {
            var element = ReflectionUtil.InstantiateSingleElementFromArray(integrationObject, propertyName);
            var cloneFromEntity = WsUtil.CloneFromEntity(element, crudOperationData);
            if (itemHandlerDelegate != null) {
                itemHandlerDelegate(cloneFromEntity, crudOperationData);
            }
        }

    }
}
