using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using softwrench.sw4.api.classes.fwk.filter;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Metadata.Exception;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {

    public class GenericSwMethodInvoker {


        public static T Invoke<T>(ApplicationSchemaDefinition schema, string stringToCheck, params object[] parameters) {
            if (stringToCheck.StartsWith("@")) {
                stringToCheck = stringToCheck.Substring(1);
            }

            if (stringToCheck.Contains(".")) {
                //generic simple injector call
                var strings = stringToCheck.Split('.');
                var serviceName = strings[0];
                var methodName = strings[1];
                var ob = SimpleInjectorGenericFactory.Instance.GetObject<object>(serviceName);
                if (ob != null) {
                    return (T)ReflectionUtil.Invoke(ob, methodName, parameters);
                }
                //shouldn´t happen as theoretically method should have been checked by existence before
                return default(T);
            }
            //if no service is defined, applying dataset implementation
            var dataSet = DataSetProvider.GetInstance().LookupDataSet(schema.ApplicationName, schema.SchemaId);
            return (T)ReflectionUtil.Invoke(dataSet, stringToCheck, parameters);
        }



        public static void CheckExistenceByString(ApplicationSchemaDefinition schema, string stringToCheck, ParameterData parameterData = null) {
            CheckExistenceByString(schema.ApplicationName, schema.SchemaId, stringToCheck, parameterData);
        }

        public static void CheckExistenceByString(string applicationName, string schemaId, string stringToCheck,
            ParameterData parameterData = null) {
            if (stringToCheck.StartsWith("@")) {
                stringToCheck = stringToCheck.Substring(1);
            }

            object service;
            string serviceName;
            string methodName;
            var alias = parameterData == null ? "" : parameterData.Alias;
            if (stringToCheck.Contains(".")) {
                if (!MetadataProvider.FinishedParsing) {
                    //cannot proceed to validate a SimpleInjector method at this point, since it´s not yet initialized
                    //TODO: move this logic placing some sort of event for things that require post-validation
                    return;
                }

                var strings = stringToCheck.Split('.');
                serviceName = strings[0];
                methodName = strings[1];
                service = SimpleInjectorGenericFactory.Instance.GetObject<object>(serviceName);
                if (service == null) {
                    throw new MetadataException("error locating reference {0}: service {1} not found".Fmt(alias, serviceName));
                }
            } else {
                service = DataSetProvider.GetInstance().LookupDataSet(applicationName, schemaId);
                methodName = stringToCheck;
                serviceName = service.GetType().Name;
            }
            var mi = ReflectionUtil.GetMethodNamed(service, methodName);
            if (mi == null) {
                throw new MetadataException("error locating reference {0}: method {1} not found on service {2} for app {3}".Fmt(alias, methodName, serviceName, applicationName));
            }
            if (parameterData == null) {
                //void methods, or no extra validation information provided
                return;
            }

            if (parameterData.Paramtypes != null) {
                var parameterInfos = mi.GetParameters();
                if (parameterInfos.Length != parameterData.Paramtypes.Count) {
                    throw new MetadataException("error locating reference {0}: method {1} on service {2} has wrong number of parameters. Expected {3}".Fmt(alias, methodName, serviceName, parameterData.Paramtypes.Count));
                }
                for (int i = 0; i < parameterInfos.Length; i++) {
                    if (!parameterData.Paramtypes[i].IsAssignableFrom(parameterInfos[i].ParameterType)) {
                        //parameters do not match
                        throw new MetadataException("error locating reference {0}: method {1} on service {2} has wrong parameters at position {3}. Expected {4}".Fmt(alias, methodName, serviceName, i, parameterData.Paramtypes[i].Name));
                    }
                }

            }
            if (parameterData.ReturnType != null && !parameterData.ReturnType.IsAssignableFrom(mi.ReturnType)) {
                //expect return does not match
                throw new MetadataException("error locating reference {0}: method {1} on service {2} has wrong return type. Expected {3}".Fmt(alias, methodName, serviceName, parameterData.ReturnType));
            }
        }


        public class ParameterData {

            public ParameterData(string alias, [CanBeNull]Type returnType, params Type[] parameters) {
                Alias = alias;
                ReturnType = returnType;
                Paramtypes = new List<Type>();
                foreach (var parameter in parameters) {
                    Paramtypes.Add(parameter);
                }
            }

            public string Alias {
                get; set;
            }


            public Type ReturnType {
                get; set;
            }

            public List<Type> Paramtypes {
                get; set;
            }

        }


    }
}
