using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Metadata.Entities.Schema;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Validator {
    internal class SWDBMetadataXmlSourceInitializer : BaseMetadataXmlSourceInitializer {



        protected override IEnumerable<EntityMetadata> InitializeEntityInternalMetadata() {
            var findTypesAnnotattedWith = AttributeUtil.FindTypesAnnotattedWith(typeof(ClassAttribute));
            var resultEntities = findTypesAnnotattedWith.Select(Convert).ToList();

            //            var subEntities = AttributeUtil.FindTypesAnnotattedWith(typeof(JoinedSubclassAttribute));
            //            foreach (var nHibernateType in subEntities) {
            //                //TODO: sub entities
            //            }

            return resultEntities;

        }

        private EntityMetadata Convert(Type type) {
            //TODO: relationships
            var name = "_" + type.Name;
            Log.DebugFormat("adding swdb internal entity {0}", name);
            return new EntityMetadata(name, GetEntitySchema(name, type), new List<EntityAssociation>(), ConnectorParameters(type));
        }

        private EntitySchema GetEntitySchema(string entityName, Type type) {
            return new EntitySchema(entityName, ConvertAttributes(entityName, type), "id", true, true, null, null, type, false);
        }

        private static IEnumerable<EntityAttribute> ConvertAttributes(string entityName, Type type) {
            var resultList = new List<EntityAttribute>();
            var connectorParameters = new ConnectorParameters(new Dictionary<string, string>(), true);
            foreach (var memberInfo in type.GetProperties()) {
                var attr = memberInfo.ReadAttribute<PropertyAttribute>();
                var query = attr == null ? null : attr.Column;
                var isId = memberInfo.ReadAttribute<IdAttribute>() != null;
                var isColumn = memberInfo.ReadAttribute<PropertyAttribute>() != null;
                var attribute = memberInfo.Name.ToLower();
                if (!memberInfo.GetMethod.ReturnType.IsPrimitive
                    && memberInfo.PropertyType != typeof(string)
                    && memberInfo.PropertyType != typeof(DateTime)
                    && memberInfo.PropertyType != typeof(DateTime?)
                    && memberInfo.PropertyType != typeof(Int32)
                    && memberInfo.PropertyType != typeof(Int64)
                    && memberInfo.PropertyType != typeof(int?)
                    && memberInfo.PropertyType != typeof(long?)
                    && memberInfo.PropertyType != typeof(int)
                    && memberInfo.PropertyType != typeof(long)
                    && !memberInfo.PropertyType.IsEnum
                    ) {
                    //TODO: components embeddables
                    //avoid adding relationships
                    continue;
                }

                if (!isId && !isColumn) {
                    //these are transient fields --> fields which are not mapped to columns, but can be used in applications
                    attribute = "#" + attribute;
                }

                Log.DebugFormat("adding swdb attribute {0} to entity {1}", attribute, entityName);
                resultList.Add(new EntityAttribute(attribute, memberInfo.PropertyType.Name, false, true,
                    connectorParameters, query));
            }


            return resultList;
        }

        private static ConnectorParameters ConnectorParameters(Type type) {
            var dictParams = new Dictionary<string, string>();
            var def = type.ReadAttribute<ClassAttribute>();
            dictParams.Add("dbtable", def.Table);
            var parameters = new ConnectorParameters(dictParams, true);
            return parameters;
        }

        protected override string MetadataPath() {
            return "swdb/swdbmetadata.xml";
        }

        protected override bool IsSWDB() {
            return true;
        }
    }
}
