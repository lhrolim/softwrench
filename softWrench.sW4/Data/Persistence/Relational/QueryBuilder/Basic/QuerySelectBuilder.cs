using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.portable.Util;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic {
    class QuerySelectBuilder : BaseQueryBuilder {

        private const string AliasDelimiter = "\"";
        private const string LiteralDelimiter = "'";

        private const string CountClause = "select count(*) as cnt ";
        private const string SelectSeparator = ", ";
        private const EntityMetadata.AttributesMode NoCollections = EntityMetadata.AttributesMode.NoCollections;

        public static string BuildSelectAttributesClause(EntityMetadata entityMetadata, QueryCacheKey.QueryMode queryMode
            , SearchRequestDto dto = null) {
            var buffer = new StringBuilder();
            if (queryMode == QueryCacheKey.QueryMode.Count) {
                return CountClause;
            }

            buffer.AppendFormat("select ");
            if (entityMetadata.FetchLimit() != null && queryMode == QueryCacheKey.QueryMode.Sync) {
                buffer.Append(string.Format(" top({0}) ", entityMetadata.FetchLimit()));
            }

            var attributes = entityMetadata.Attributes(NoCollections) as IList<EntityAttribute>
                ?? entityMetadata.Attributes(NoCollections).ToList();

            var hasProjection = dto != null && dto.ProjectionFields.Count > 0;
            if (hasProjection) {
                foreach (ProjectionField field in dto.ProjectionFields) {
                    if (field.Name.StartsWith("#")) {
                        if (field.Name.StartsWith("#null")) {
                            //this way we can map null attributes, that can be used for unions
                            //see changeunionschema of hapag´s metadata.xml
                            buffer.AppendFormat("null" + SelectSeparator);
                        } else {
                            //this is an unmapped attribute
                            continue;
                        }
                    }
                    //Item1 = Attribute, item2 = relationshipname
                    var result = entityMetadata.LocateNonCollectionAttribute(field.Name, attributes);
                    if ((!field.Name.Contains('.') && result == null) || (result != null && result.Item1 == null)) {
                        //second condition shouldn't happen unless due to a metadata misconfiguration
                        //this field is not mapped
                        continue;
                    }
                    string aliasAttribute;
                    if (result.Item1.Query != null) {
                        aliasAttribute = AliasAttributeWithQuery(entityMetadata, field.Alias, result.Item1, result.Item2);
                    } else {
                        aliasAttribute = AliasAttribute(entityMetadata, field);
                    }
                    buffer.AppendFormat(aliasAttribute + SelectSeparator);
                }

            } else {
                for (var i = 0; i < attributes.Count; i++) {
                    var entityAttribute = attributes[i];
                    if (entityAttribute.Name.StartsWith("#null")) {
                        //this way we can map null attributes, that can be used for unions
                        //see changeunionschema of hapag´s metadata.xml
                        buffer.AppendFormat("null" + SelectSeparator);
                    } else {
                        var aliasAttribute = AliasAttribute(entityMetadata, entityAttribute);
                        buffer.AppendFormat(aliasAttribute + SelectSeparator);
                    }
                }
            }
            return buffer.ToString().Substring(0, buffer.Length - SelectSeparator.Count()) + " ";
        }

       

        private static string AliasAttributeWithQuery(EntityMetadata entityMetadata, string alias, EntityAttribute attribute, string context) {
            var contextToUse = context ?? entityMetadata.Name;
            var query = attribute.Query;
            var qualifiedName = "";
            if (query.StartsWith("@")) {
                qualifiedName = GetServiceQuery(query, contextToUse);
            } else {
                qualifiedName = attribute.GetQueryReplacingMarkers(contextToUse);
            }
            return string.Format("{0} as {1}", qualifiedName, alias);
        }

        private static string AliasAttribute(EntityMetadata entityMetadata, ProjectionField projectionField) {
            var name = projectionField.Name;
            var qualifiedName = name.IndexOf('.') != -1 ? name : string.Format("{0}.{1}", entityMetadata.Name, name);
            var alias = projectionField.Alias;
            return string.Format("{0} as {1}", qualifiedName, alias);
        }


        private static string AliasAttribute(EntityMetadata entityMetadata, EntityAttribute attribute) {
            var qualifiedName = BaseQueryUtil.QualifyAttribute(entityMetadata, attribute);
            var alias = string.Format("{0}{1}{0}", AliasDelimiter, attribute.Name);
            if (attribute.Query != null) {
                return AliasAttributeWithQuery(entityMetadata, alias, attribute, null);
            }
            return string.Format("{0} as {1}", qualifiedName, alias);
        }




    }
}
