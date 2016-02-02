using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.SimpleInjector;
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