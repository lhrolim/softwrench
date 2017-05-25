using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using cts.commons.persistence.Util;
using cts.commons.portable.Util;
using Castle.Core.Internal;
using NHibernate.Mapping.Attributes;
using NHibernate.Mapping.ByCode;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Metadata.Entities.Schema;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Validator {
    internal class SWDBMetadataXmlSourceInitializer : BaseMetadataXmlSourceInitializer {



        protected override IEnumerable<EntityMetadata> InitializeEntityInternalMetadata() {
            var findTypesAnnotattedWith = AttributeUtil.FindTypesAnnotattedWith(AssemblyLocator.GetSWAssemblies(), typeof(ClassAttribute), typeof(JoinedSubclassAttribute));
            var resultEntities = findTypesAnnotattedWith.Select(Convert).ToList();

            //            var subEntities = AttributeUtil.FindTypesAnnotattedWith(typeof(JoinedSubclassAttribute));
            //            foreach (var nHibernateType in subEntities) {
            //                //TODO: sub entities
            //            }

            return resultEntities;

        }

        public EntityMetadata Convert(Type type) {
            //TODO: relationships
            var name = type.Name + "_";
            Log.DebugFormat("adding swdb internal entity {0}", name);
            var properties = ParseProperties(name, type);
            var idAttribute = properties.Item3 ?? "id";
            var userIdAttribute = properties.Item4 ?? idAttribute;
            var entitySchema = new EntitySchema(name, properties.Item1, idAttribute, userIdAttribute, true, true, null, null, type, false);
            return new EntityMetadata(name, entitySchema, properties.Item2, ConnectorParameters(type), type);
        }

        private static Tuple<IEnumerable<EntityAttribute>, IEnumerable<EntityAssociation>, string, string> ParseProperties(string entityName, Type type) {

            var resultAttributes = new List<EntityAttribute>();
            var resultAssociations = new List<EntityAssociation>();
            PropertyInfo idAttribute = null;
            var idAttributeName = "id";
            var isJoinedSubclass = type.ReadAttribute<JoinedSubclassAttribute>() != null;
            var userIdAttributeName = (string) null;


            var connectorParameters = new ConnectorParameters(new Dictionary<string, string>(), true);
            foreach (var memberInfo in type.GetProperties()) {
                var attr = memberInfo.ReadAttribute<PropertyAttribute>();
                var query = attr?.Column;
                var isId = memberInfo.ReadAttribute<IdAttribute>() != null;
                var isJoinedSubClassId = isJoinedSubclass && memberInfo.ReadAttribute<KeyAttribute>() != null && memberInfo.GetCustomAttributes(true).Length == 1;
                if (isJoinedSubClassId) {
                    idAttributeName = memberInfo.ReadAttribute<KeyAttribute>().Column;
                }
                var isColumn = memberInfo.ReadAttribute<PropertyAttribute>() != null;
                var attribute = memberInfo.Name.ToLower();
                if (IsNotAPrimitiveType(memberInfo)) {
                    //avoid adding relationships
                    var oneTomany = memberInfo.ReadAttribute<OneToManyAttribute>();
                    var manyToOne = memberInfo.ReadAttribute<ManyToOneAttribute>();
                    if (oneTomany != null) {
                        resultAssociations.Add(HandleOneToMany(memberInfo, oneTomany, idAttribute));
                    }
                    if (manyToOne != null) {
                        resultAttributes.Add(HandleManyToOneHiddenAttribute(memberInfo, manyToOne));
                    }

                    var embeddable = memberInfo.ReadAttribute<ComponentPropertyAttribute>();
                    if (embeddable != null) {
                        resultAttributes.AddRange(HandleEmbedabble(memberInfo));
                    }

                    //TODO: other relationships like many to one

                    continue;
                }

                if (memberInfo.GetAttribute<UserIdProperty>() != null) {
                    userIdAttributeName = memberInfo.Name;
                }

                if (!isId && !isColumn && !isJoinedSubClassId) {
                    //these are transient fields --> fields which are not mapped to columns, but can be used in applications
                    attribute = "#" + attribute;
                } else if (isId) {
                    if (!isJoinedSubclass) {
                        idAttribute = memberInfo;
                        idAttributeName = idAttribute.Name;
                    }
                }

                Log.DebugFormat("adding swdb attribute {0} to entity {1}", attribute, entityName);
                var entityAttribute = new EntityAttribute(attribute, memberInfo.PropertyType.Name, false, true, connectorParameters, query);
                if (entityAttribute.Type.EqualsIc("datetime") && memberInfo.ReadAttribute<UTCDateTime>() != null) {
                    entityAttribute.ConnectorParameters.Parameters.Add("utcdate", "true");
                }
                resultAttributes.Add(entityAttribute);
            }
            return new Tuple<IEnumerable<EntityAttribute>, IEnumerable<EntityAssociation>, string, string>(resultAttributes, resultAssociations, idAttributeName, userIdAttributeName);
        }

        private static EntityAttribute HandleManyToOneHiddenAttribute(PropertyInfo memberInfo, ManyToOneAttribute manyToOne) {
            var defaultInstance = Metadata.Entities.Connectors.ConnectorParameters.DefaultInstance();
            var idProperty = memberInfo.PropertyType.FindPropertiesWithAttribute(typeof(IdAttribute));
            var idPropertyType = "int";
            if (idProperty != null && idProperty.Any()) {
                idPropertyType = typeof(string) == idProperty.First().PropertyType ? "varchar" : "int";
            }

            return new EntityAttribute(manyToOne.Column, idPropertyType, false, true, defaultInstance, null);
        }

        private static bool IsNotAPrimitiveType(PropertyInfo memberInfo)
        {
            return !memberInfo.GetMethod.ReturnType.IsPrimitive
                   && memberInfo.PropertyType != typeof (string)
                   && memberInfo.PropertyType != typeof (DateTime)
                   && memberInfo.PropertyType != typeof (DateTime?)
                   && memberInfo.PropertyType != typeof (Int32)
                   && memberInfo.PropertyType != typeof(bool?)
                   && memberInfo.PropertyType != typeof (Int64)
                   && memberInfo.PropertyType != typeof (int?)
                   && memberInfo.PropertyType != typeof (long?)
                   && memberInfo.PropertyType != typeof (int)
                   && memberInfo.PropertyType != typeof (long)
                   && memberInfo.PropertyType != typeof (byte[])
                   && !memberInfo.PropertyType.IsEnumOrNullableEnum();


        }

        private static IEnumerable<EntityAttribute> HandleEmbedabble(PropertyInfo memberInfo) {
            var embeddableType = memberInfo.PropertyType;
            var tuple = ParseProperties(embeddableType.Name, embeddableType);
            return tuple.Item1;


        }

        private static EntityAssociation HandleOneToMany(PropertyInfo memberInfo, OneToManyAttribute oneTomany, PropertyInfo idAttribute) {
            var keyAttr = memberInfo.ReadAttribute<KeyAttribute>();
            var qualifier = memberInfo.Name.ToLower();
            var to = oneTomany.ClassType.Name.ToLower() + "_";
            //            //TODO: Add reverse customization
            //            string reverse = null;
            IList<EntityAssociationAttribute> attributes = new List<EntityAssociationAttribute>();
            var idAttributeName = idAttribute?.Name.ToLower() ?? "id";

            attributes.Add(new EntityAssociationAttribute(keyAttr.Column, idAttributeName, null, true));
            return new EntityAssociation(qualifier + "_", to, attributes, true, false, false, null, false, false);
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
