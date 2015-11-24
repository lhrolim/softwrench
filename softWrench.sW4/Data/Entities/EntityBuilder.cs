using cts.commons.portable.Util;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Data.Entities {


    public class EntityBuilder {

        private const string RelationshipNotFound = "relationship {0} not found on entity {1}";

        private static readonly ILog Log = LogManager.GetLogger(typeof(EntityBuilder));



        public static T BuildFromJson<T>(Type entityType, EntityMetadata metadata, ApplicationMetadata applicationMetadata, JObject json, string id=null) where T : Entity {
            if (id == null && applicationMetadata!=null) {
                //the id can be located inside the json payload, as long as the application metadata is provider
                //this is not the case for new item compositions though
                JToken token;
                json.TryGetValue(applicationMetadata.IdFieldName, out token);
                if (token != null) {
                    id = token.ToString();
                }
            }

            var entity = GetInstance(entityType, metadata, applicationMetadata, id);
            OperationType operationType = id == null ? OperationType.Add : OperationType.AddChange;
            foreach (var property in json.Properties()) {
                PopulateEntity<T>(metadata, applicationMetadata, property, entity, entityType, operationType);
            }
            return (T)entity;
        }

        private static void PopulateEntity<T>(EntityMetadata metadata, ApplicationMetadata applicationMetadata,
            JProperty property, Entity entity, Type entityType, OperationType operationType)
            where T : Entity {
            var attributes = entity.Attributes;
            var associationAttributes = entity.AssociationAttributes;
            var name = property.Name;
            var listAssociations = metadata.ListAssociations();
            var collectionAssociation = listAssociations.FirstOrDefault(l => l.Qualifier == EntityUtil.GetRelationshipName(name));
            if (name.Contains(".")) {
                HandleRelationship<T>(entityType, metadata, associationAttributes, property);
                var attribute = metadata.Schema.Attributes.FirstOrDefault(a => a.Name == name);
                if (attribute != null) {
                    attributes.Add(property.Name, GetValueFromJson(GetType(metadata, attribute), property.Value));
                }
            } else if (collectionAssociation != null) {
                HandleCollections<T>(entityType, metadata, applicationMetadata, collectionAssociation, associationAttributes, property);
            } else {
                var attribute = metadata.Schema.Attributes.FirstOrDefault(a => a.Name == name);
                // if we´re on add mode, make sure the id isn´t setted 
                if (operationType == OperationType.Add && Equals(metadata.Schema.IdAttribute, attribute)) {
                    return;
                }
                if (attribute != null && !attributes.ContainsKey(property.Name)) {
                    var type = GetType(metadata, attribute);
                    try {
                        var valueFromJson = GetValueFromJson(type, property.Value);
                        attributes.Add(property.Name, valueFromJson);
                    } catch (Exception e) {
                        Log.Error("error casting object", e);
                        throw new InvalidCastException("wrong configuration for field {0} throwing cast exception. MetadataType:{1} / Value:{2}".Fmt(attribute.Name, type, property.Value));
                    }

                } else if (property.Value.Type == JTokenType.Array) {
                    var array = property.Value.ToObject<Object[]>();
                    entity.UnmappedAttributes.Add(property.Name, String.Join(", ", array));
                } else {
                    var value = property.Value.Type == JTokenType.Null ? null : property.Value.ToString();
                    if (value == "$null$ignorewatch") {
                        value = null;
                    }
                    entity.UnmappedAttributes.Add(property.Name, value);
                }
            }
        }

        private static string GetType(EntityMetadata metadata, EntityAttribute attribute) {
            if (metadata.Targetschema == null) {
                return attribute.Type;
            }
            var targetMapping = metadata.Targetschema.TargetAttributes.FirstOrDefault(a => a.Name.Equals(attribute.Name, StringComparison.CurrentCultureIgnoreCase));
            return targetMapping != null ? targetMapping.Type : attribute.Type;
        }


        private static void HandleCollections<T>(Type entityType, EntityMetadata metadata, ApplicationMetadata applicationMetadata, EntityAssociation collectionAssociation,
            IDictionary<string, object> associationAttributes, JProperty property) where T : Entity {
            var collectionType = metadata.RelatedEntityMetadata(collectionAssociation.Qualifier);
            var array = (JArray)property.Value;
            IList<T> collection = new List<T>();

            foreach (JObject jToken in array) {
                JToken idTkn;
                String idValue = null;
                String userIdValue = null;
                if (jToken.TryGetValue(collectionType.Schema.IdAttribute.Name, out idTkn)) {
                    var valueFromJson = GetValueFromJson(collectionType.Schema.IdAttribute.Type, idTkn);
                    idValue = valueFromJson == null ? null : valueFromJson.ToString();
                }
                if (jToken.TryGetValue(collectionType.Schema.UserIdAttribute.Name, out idTkn)) {
                    var valueFromJson = GetValueFromJson(collectionType.Schema.UserIdAttribute.Type, idTkn);
                    userIdValue = valueFromJson == null ? null : valueFromJson.ToString();
                }

                var entity = BuildFromJson<T>(entityType, collectionType, null, jToken, idValue);
                collection.Add(entity);
            }
            associationAttributes.Add(EntityUtil.GetRelationshipName(property.Name), collection);
        }

        private static void HandleRelationship<T>(Type entityType, EntityMetadata metadata,
            IDictionary<string, object> associationAttributes, JProperty property) where T : Entity {
            string attributeName;
            var relationshipName = EntityUtil.GetRelationshipName(property.Name, out attributeName);
            var relationship = metadata.RelatedEntityMetadata(relationshipName);
            if (relationship == null) {
                //                throw new InvalidOperationException(String.Format(RelationshipNotFound, relationshipName, metadata.Name));
                return;
            }
            object relatedEntity;
            if (!associationAttributes.TryGetValue(relationshipName, out relatedEntity)) {
                //TODO: why is that null?
                relatedEntity = GetInstance(entityType, relationship, null, null);
                associationAttributes.Add(relationshipName, relatedEntity);
            }
            //TODO: last parameter might have been setted based on a flag sent from the json, indicating the correct relationship mode
            PopulateEntity<T>(relationship, null, new JProperty(attributeName, property.Value), (Entity)relatedEntity, entityType, OperationType.Change);
        }



        private static Entity GetInstance(Type entityType, EntityMetadata metadata, ApplicationMetadata applicationMetadata, string id) {
            if (entityType == typeof(CrudOperationData)) {
                return new CrudOperationData(id, new Dictionary<string, object>(),
                                                      new Dictionary<string, object>(), metadata, applicationMetadata);
            }
            return new Entity(id, new Dictionary<string, object>(), new Dictionary<string, object>(), metadata);
        }





        private static object GetValueFromJson(string type, JToken value) {
            //TODO: review.
            if (null == value) {
                return null;
            }
            if (value.Type != JTokenType.Array) {

                var stValue = value.ToString();
                if (stValue == "") {
                    if (type.EqualsAny("datetime", "timestamp")) {
                        return null;
                    }

                    return value.Type == JTokenType.Null ? null : value.ToString();
                }
                if (stValue == "$null$ignorewatch") {
                    return null;
                }
                return ConversionUtil.ConvertFromMetadataType(type, stValue);

            }
            var array = value.ToObject<Object[]>();
            for (var i = 0; i < array.Length; i++) {
                if (array[i] != null) {
                    array[i] = ConversionUtil.ConvertFromMetadataType(type, array[i].ToString());
                }
            }

            if (array.Length == 0) {
                return null;
            }

            return array;
        }
    }
}
