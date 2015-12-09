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
using r = softWrench.sW4.Util.ReflectionUtil;

namespace softWrench.sW4.Data.Persistence.WS.Internal {
    public class WsUtil {
        //        private const String MEA_ENTITIES_PACKAGE = "softWrench.sW4.Data.Persistence.WS.Mea.Entities";
        //        private const String MIF_ENTITIES_PACKAGE = "softWrench.sW4.Data.Persistence.WS.Mif.Entities";

        private static readonly ILog Log = LogManager.GetLogger(typeof(WsUtil));

        public static WsProvider WsProvider() {
            var target = MetadataProvider.TargetMapping();
            if (target == "maximo7.5" || target == "maximo7.1" || target == "maximo7.2.1" || target == "smartcloud7.5") {
                return Constants.WsProvider.MIF;
            }
            if (target == "ism") {
                return Constants.WsProvider.ISM;
            }
            if (target.StartsWith("maximo6")) {
                return Constants.WsProvider.MEA;
            }
            var prop = MetadataProvider.GlobalProperty("wsprovider", true);
            WsProvider val;
            Enum.TryParse(prop, true, out val);
            return val;
        }

        public static Boolean Is75() {
            var target = MetadataProvider.TargetMapping();
            return target.Contains("7.5");
        }

        public static Boolean Is71() {
            var target = MetadataProvider.TargetMapping();
            return target.Contains("7.1");
        }

        public static object GetRealValue(Object integrationObject, string propertyName) {
            object property = ReflectionUtil.GetProperty(integrationObject, propertyName);
            if (property == null) {
                return null;
            }
            var value = ReflectionUtil.GetProperty(property, "Value");
            return value;
        }

        public static T GetRealValue<T>(Object integrationObject, string propertyName) {
            object property = ReflectionUtil.GetProperty(integrationObject, propertyName);
            if (property == null) {
                return default(T);
            }
            var value = ReflectionUtil.GetProperty(property, "Value");
            return (T)value;
        }

        public static T GetRealValue<T>(Object mxObject) where T : class {
            var value = ReflectionUtil.GetProperty(mxObject, "Value");
            return (T)value;
        }

        public static void SetAction(object baseObjectWithAction, OperationType actionType) {
            ReflectionUtil.SetProperty(baseObjectWithAction, "action", actionType.ToString());
            ReflectionUtil.SetProperty(baseObjectWithAction, "actionSpecified", true);
        }


        public static void SetChanged(object baseObjectWithAction) {
            ReflectionUtil.SetProperty(baseObjectWithAction, "changed", true);
            ReflectionUtil.SetProperty(baseObjectWithAction, "changedSpecified", true);
        }

        public static void SetChanged(params object[] baseObjectWithAction) {
            foreach (object ob in baseObjectWithAction) {
                SetChanged(ob);
            }

        }

        public static object SetValueIfNull(object baseObject, string propertyName, object value,
            Boolean markSpecified = false, bool setIfNegativeToo = false) {
            if (baseObject == null) {
                Log.Warn(String.Format("property {0} not found on object null", propertyName));
                return null;
            }
            PropertyDescriptor property = ReflectionUtil.GetPropertyDescriptor(baseObject, propertyName);
            if (property == null) {
                Log.Warn(String.Format("property {0} not found on object {1}", propertyName, baseObject.GetType()));
                return null;
            }
            var propertyVal = property.GetValue(baseObject);
            var currentValue = ReflectionUtil.GetProperty(propertyVal, "Value");
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

        public static object SetValue(object baseObject, string propertyName, object value, Boolean markSpecified = false) {
            var propDescriptor = ReflectionUtil.PropertyDescriptor(baseObject, propertyName);
            if (propDescriptor == null) {
                if (ApplicationConfiguration.IsLocal()) {
                    Log.WarnFormat("property {0} not found on object {1}. Review metadata config or maximo config",
                        propertyName, baseObject);
                }
                return null;
            }
            Type propertyType = propDescriptor.PropertyType;
            bool isPrimitive = propertyType.IsPrimitive || typeof(DateTime).IsAssignableFrom(propertyType) || propertyType == typeof(string);
            if (isPrimitive) {
                propDescriptor.SetValue(baseObject, value);
                if (markSpecified) {
                    SetValue(baseObject, propertyName + "Specified", true);
                }
                return value;
            }
            var prop = propDescriptor.GetValue(baseObject);
            if (prop == null) {
                return ReflectionUtil.InstantiateProperty(baseObject, propertyName, new { Value = value });
            }
            ReflectionUtil.SetProperty(prop, new { Value = value });
            if (markSpecified) {
                SetValue(baseObject, propertyName + "Specified", true);
            }
            return prop;
        }



        public static object SetQueryValue(object baseObject, string propertyName, object value, QueryOperator operatorType = QueryOperator.Equals) {
            object queryField = SetValue(baseObject, propertyName, value);
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
            PropertyDescriptorCollection targetProperties = TypeDescriptor.GetProperties(target);
            foreach (PropertyDescriptor prop in targetProperties) {
                var value = attributes.GetAttribute(prop.Name.ToLower());
                if (value == null) continue;
                if (prop.PropertyType.IsPrimitive) {
                    prop.SetValue(target, value);
                } else if (!prop.PropertyType.IsPrimitive) {
                    object o = prop.GetValue(target);
                    if (o == null) {
                        object newInstance = ReflectionUtil.InstanceFromType(prop.PropertyType);
                        prop.SetValue(target, newInstance);
                        o = newInstance;
                    }
                    ReflectionUtil.SetProperty(o, new { Value = value });
                }
            }
            return target;
        }

        public static void NullifyValue(object baseObject, string propertyName) {
            ReflectionUtil.SetProperty(baseObject, propertyName, null);
        }

        public static void CloneArray(IEnumerable<CrudOperationData> collectionAssociation, object integrationObject, string propertyName, Action<object, CrudOperationData> itemHandlerDelegate = null) {
            var crudOperationDatas = collectionAssociation as IList<CrudOperationData> ?? collectionAssociation.ToList();
            if (!crudOperationDatas.Any()) {
                return;
            }
            var arr = ReflectionUtil.InstantiateArrayWithBlankElements(integrationObject, propertyName, crudOperationDatas.Count);
            for (var i = 0; i < arr.Length; i++) {
                CrudOperationData crudOperationData = crudOperationDatas[i];
                var cloneFromEntity = WsUtil.CloneFromEntity(arr.GetValue(i), crudOperationData);
                if (itemHandlerDelegate != null) {
                    itemHandlerDelegate(cloneFromEntity, crudOperationData);
                }
                //                WsUtil.SetValue(cloneFromEntity, "worklogid", -1);
                arr.SetValue(cloneFromEntity, i);
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
