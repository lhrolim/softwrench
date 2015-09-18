using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using cts.commons.persistence.Util;
using cts.commons.portable.Util;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Metadata.Entities.Schema;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Validator {
    internal class SWDBMetadataXmlSourceInitializer : BaseMetadataXmlSourceInitializer {



        protected override IEnumerable<EntityMetadata> InitializeEntityInternalMetadata() {
            var findTypesAnnotattedWith = AttributeUtil.FindTypesAnnotattedWith(typeof(ClassAttribute),
                typeof(JoinedSubclassAttribute));
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
            var properties = ParseProperties(name, type);
            var entitySchema = new EntitySchema(name, properties.Item1, "id", "id", true, true, null, null, type, false);
            return new EntityMetadata(name, entitySchema, properties.Item2, ConnectorParameters(type));
        }

        private static Tuple<IEnumerable<EntityAttribute>, IEnumerable<EntityAssociation>> ParseProperties(string entityName, Type type) {

            var resultAttributes = new List<EntityAttribute>();
            var resultAssociations = new List<EntityAssociation>();

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
                    && memberInfo.PropertyType != typeof(byte[])
                    && !memberInfo.PropertyType.IsEnum
                    ) {
                    //TODO: components embeddables
                    //avoid adding relationships
                    var oneTomany = memberInfo.ReadAttribute<OneToManyAttribute>();
                    if (oneTomany != null) {
                        resultAssociations.Add(HandleOneToMany(memberInfo, oneTomany));
                    }
                    //TODO: other relationships like many to one

                    continue;
                }

                if (!isId && !isColumn) {
                    //these are transient fields --> fields which are not mapped to columns, but can be used in applications
                    attribute = "#" + attribute;
                }

                Log.DebugFormat("adding swdb attribute {0} to entity {1}", attribute, entityName);
                var entityAttribute = new EntityAttribute(attribute, memberInfo.PropertyType.Name, false, true, connectorParameters, query);
                if (entityAttribute.Type.EqualsIc("datetime") && memberInfo.ReadAttribute<UTCDateTime>() != null) {
                    entityAttribute.ConnectorParameters.Parameters.Add("utcdate", "true");
                }
                resultAttributes.Add(entityAttribute);
            }
            return new Tuple<IEnumerable<EntityAttribute>, IEnumerable<EntityAssociation>>(resultAttributes, resultAssociations);
        }

        private static EntityAssociation HandleOneToMany(PropertyInfo memberInfo, OneToManyAttribute oneTomany) {
            var keyAttr = memberInfo.ReadAttribute<KeyAttribute>();
            var qualifier = memberInfo.Name;
            var to = "_" + oneTomany.ClassType.Name.ToLower();
            //TODO: Add reverse customization
            string reverse = null;
            IList<EntityAssociationAttribute> attributes = new List<EntityAssociationAttribute>();
            attributes.Add(new EntityAssociationAttribute(keyAttr.Column, "id", null, true));
            return new EntityAssociation("_" + qualifier, to, attributes, true, reverse, false);
        }

        private static ConnectorParameters ConnectorParameters(Type type) {
            var dictParams = new Dictionary<string, string>();
            if (type.ReadAttribute<ClassAttribute>() != null) {
                dictParams.Add("dbtable", type.ReadAttribute<ClassAttribute>().Table);
            } else {
                dictParams.Add("dbtable", type.ReadAttribute<JoinedSubclassAttribute>().Table);
            }

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
