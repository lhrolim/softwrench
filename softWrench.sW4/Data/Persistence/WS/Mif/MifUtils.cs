using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;

namespace softWrench.sW4.Data.Persistence.WS.Mif
{


    internal class MifUtils
    {

        private const string DataTypeConnectorParameter = "mifDataType";

        private static readonly object[] baseArray = new object[] { true, MifConstants.LANGCODE, MifConstants.LANGCODE, string.Empty, string.Empty };
        private static readonly object[] queryParamArray = new object[] { true, "10", "0", string.Empty, string.Empty };

        

        public static object[] GetParameterList(object firstParam)
        {
            object[] obArr = new object[7];
            obArr.SetValue(firstParam,0);
            obArr.SetValue(DateTime.Now, 1);
            obArr.SetValue(true, 2);
            obArr.SetValue(MifConstants.LANGCODE, 3);
            obArr.SetValue(MifConstants.LANGCODE, 4);
            obArr.SetValue(String.Empty, 5);
            obArr.SetValue(String.Empty, 6);
            return obArr;
        }

        public static object PopulateWsEntityFromJson(Object dbEntity, EntityMetadata metadata, Entity json)
        {
            foreach (var property in json)
            {
                var isAssociated = property
                    .Key
                    .Contains(EntityAttribute.AttributeQualifierSeparator);

                if (false == isAssociated)
                {
                    var key = property.Key;
                    DoSetValue(dbEntity, key, GetNewMxType(metadata, key, property.Value));
                }
            }
            return dbEntity;
        }

        public static void DoSetValue(object dbEntity, String key, object value)
        {
            PropertyDescriptor prop = TypeDescriptor.GetProperties(dbEntity)[key.ToUpper()];

            if (value != null)
            {
                prop.SetValue(dbEntity, value);
            }
        }

        public static void DoSetValue(object dbEntity, EntityMetadata metadata, String key, object value,string mxType)
        {
            DoSetValue(dbEntity, key, GetNewMxType(metadata, key, value, mxType));
        }

        public static IList<object> GetParameterListForQuery(object firstParam)
        {
            var parameterList = GetParameterList(firstParam);
            IEnumerable<object> enumerable = parameterList.Concat(queryParamArray);
            return enumerable.ToList();
        }

        public static object GetNewMxType(EntityMetadata entity, string key, object value, string mxType = null)
        {
            var convertedValue = value;
            if (mxType == null)
            {
                var attribute = entity
                    .Schema
                    .Attributes
                    .First(a => a.Name == key);

                mxType = GetMXTypeFromMetadata(attribute, key);
                convertedValue = getValueFromJson(mxType, (JToken) value);
            }
            if (mxType == null)
            {
                return null;
            }
            return GetNewFixedMxType(entity, mxType, convertedValue);

        }

        public static object GetNewFixedMxType(EntityMetadata entity, string mxtype, object value)
        {
            string typeName = MifMethodNameUtils.GetBaseWsTypeName(entity.Name) + "." + mxtype;
            var ob = Activator.CreateInstance(null, typeName).Unwrap();
            PropertyDescriptor prop = TypeDescriptor.GetProperties(ob)["Value"];
            prop.SetValue(ob, value);
            return ob;
        }

        private static string GetMXTypeFromMetadata(EntityAttribute attribute, string key)
        {         
           var type = attribute
               .ConnectorParameters
               .Parameters[DataTypeConnectorParameter];

           //TODO: complement
           return type.StartsWith("MX") ? type : null;
        }

        private static object getValueFromJson(String type, JToken value)
        {
            //TODO: review.
            string stValue = value.ToString();
            if (type == "MXStringType")
            {
                return stValue;
            }

            if (stValue == "")
            {
                return null;
            }

            if (type == "MXLongType")
            {
                return Convert.ToInt64(stValue);
            }
            else if (type == "MXDateTimeType")
            {
                return Convert.ToDateTime(stValue); 
            }
            return stValue;
        }


        public static object getNewWSEntity(string entity)
        {
             return Activator.CreateInstance(null, MifMethodNameUtils.GetWSEntityName(entity)).Unwrap(); 
        }
    }
}
