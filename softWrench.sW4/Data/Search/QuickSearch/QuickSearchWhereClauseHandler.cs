using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using log4net;
using log4net.Repository.Hierarchy;
using Quartz.Util;
using softwrench.sw4.Shared2.Metadata.Applications.Filter;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Search.QuickSearch {
    public class QuickSearchWhereClauseHandler : ISingletonComponent {

        private readonly QuickSearchHelper _quickSearchHelper;

        private static readonly ILog Log = LogManager.GetLogger(typeof(QuickSearchWhereClauseHandler));

        public QuickSearchWhereClauseHandler(QuickSearchHelper quickiSearchHelper) {
            Log.Debug("init..");
            _quickSearchHelper = quickiSearchHelper;
        }


        /// <summary>
        /// Appends whereclause and it's query parameter on the dto for a quick search 
        /// ('or' and like in all declared filters's attributes).
        /// The query parameter will have the value of the dto's QuickSearchData property.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public virtual PaginatedSearchRequestDto HandleDTO(ApplicationSchemaDefinition schema, PaginatedSearchRequestDto dto) {
            if (dto == null || dto.QuickSearchDTO == null) {
                return dto;
            }
            // iterate filters and 'OR' the attributes

            var entity = MetadataProvider.Entity(schema.EntityName);
            //caching this call
            

            var schemaFilters = schema.SchemaFilters;
            if (schemaFilters == null) {
                return dto;
            }

            // filter out datetime and boolean filters
            var filtersToApply = schemaFilters.Filters
                .Where(f => !(f is MetadataBooleanFilter) && !(f is MetadataDateTimeFilter) && !(f is MetadataNumberFilter) && !StatusFilter(f.Attribute, schema));

            var validFilterAttributes = filtersToApply
                    .Select(f => AttribteAppendingApplicationPrefix(schema,f.Attribute, entity,  schema.Fields)).Where(f => f!= null);

            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append(_quickSearchHelper.BuildOrWhereClause(schema.DbType, validFilterAttributes));


            // ReSharper disable once PossibleNullReferenceException
            if (dto.QuickSearchDTO.CompositionsToInclude != null) {
                sb.Append(HandleCompositions(schema, dto.QuickSearchDTO.CompositionsToInclude));
            }
            sb.Append(")");

            // appending just where clause statement: value of the statement parameter is set at SearchUtils#GetParameters
            dto.AppendWhereClause(sb.ToString());

            return dto;
        }

        private bool StatusFilter(string attribute, ApplicationSchemaDefinition schema) {
            var attributeDef = schema.Fields.FirstOrDefault(f => f.Attribute.EqualsIc(attribute));
            if (attributeDef == null) {
                return false;
            }
            return attributeDef.RendererType != null && attributeDef.RendererType.EqualsIc("statusicons");
        }


        private string HandleCompositions(ApplicationSchemaDefinition schema, IEnumerable<string> compositionsToInclude) {
            var outerBuider = new StringBuilder();
            var detailSchema = MetadataProvider.LocateRelatedDetailSchema(schema);
            if (detailSchema == null) {
                Log.WarnFormat("unable to locate detail schema, ignoring query");
                return "";
            }
            foreach (var item in compositionsToInclude) {
                var relationshipName = EntityUtil.GetRelationshipName(item);
                //we´re on a list schema --> first need to locate related detail schema
                var composition = detailSchema.Compositions().FirstOrDefault(f => f.Relationship.EqualsIc(relationshipName));
                if (composition == null) {
                    Log.WarnFormat("composition {0} not found on schema {1}. Ignoring. check you client implementation", item, schema.GetApplicationKey());
                    continue;
                }

                var handleSingleCompositionQuery = HandleSingleComposition(composition, schema.EntityName);
                if (!string.IsNullOrEmpty(handleSingleCompositionQuery)) {
                    outerBuider.Append(" or exists (").Append(handleSingleCompositionQuery).Append(")");
                }
            }
            return outerBuider.ToString();
        }

        [NotNull]
        private string HandleSingleComposition(ApplicationCompositionDefinition composition, string entityName) {
            var listCompositionSchema = composition.Schema.Schemas.List;
            var sb = new StringBuilder();
            var entityAssociation = composition.EntityAssociation;
            var aliased = BaseQueryUtil.AliasEntity(listCompositionSchema.EntityName, entityAssociation.Qualifier);
            sb.AppendFormat("select 1 from {0} where ", aliased);
            var entityMetadata = MetadataProvider.Entity(entityName);
            var baseJoinConditions = QueryJoinBuilder.AppendJoinConditions(entityMetadata, entityAssociation);
            sb.Append(baseJoinConditions);

            var compositionEntity = MetadataProvider.Entity(listCompositionSchema.EntityName);

            //quicksearch fields
            var quickSearchFields = listCompositionSchema.GetProperty(ApplicationSchemaPropertiesCatalog.ListQuickSearchFields);
            var first = true;

            if (quickSearchFields != null) {
                if (string.IsNullOrWhiteSpace(quickSearchFields)) {
                    //limit case where the composition is declared to contain no fields --> disregard this composition
                    return "";
                }

                var fieldsToInclude = quickSearchFields.Split(',');
                foreach (var fieldToInclude in fieldsToInclude) {
                    var fieldDefinition =
                        listCompositionSchema.Fields.FirstOrDefault(f => f.Attribute.EqualsIc(fieldToInclude));
                    if (fieldDefinition == null) {
                        Log.WarnFormat("field {0} could not be located at {1}, review your quicksearch config", fieldToInclude, listCompositionSchema.ApplicationName);
                        continue;
                    }

                    sb.Append(first ? " and (" : " or ");
                    first = false;
                    var ignoreCoalesce = IgnoreCoalesce(fieldDefinition);
                    sb.Append(QuickSearchHelper.QuickSearchStatement(entityAssociation.Qualifier + "." + fieldToInclude, entityAssociation.DbType, ignoreCoalesce));
                }

                if (!first) {
                    //at least one entry was found, closing parenthesis
                    sb.Append(")");
                }
            } else {
                var nonHiddens = listCompositionSchema.NonHiddenFields.Where(f => f.IsTextField);
                first = true;
                foreach (var nonHidden in nonHiddens) {
                    var attributeName = nonHidden.Attribute;
                    var entityAttribute = compositionEntity.Schema.Attributes.FirstOrDefault(f => f.Name.EqualsIc(attributeName));
                    if (entityAttribute == null) {
                        Log.DebugFormat("transient field {0} ignored for quick search query", attributeName);
                        continue;
                    }
                    sb.Append(first ? " and (" : " or ");
                    first = false;
                    var queryAttribute = BaseQueryUtil.ParseAttributeForQuery(compositionEntity,
                        entityAssociation.Qualifier, entityAttribute, entityAssociation.Qualifier);

                    var ignoreCoalesce = IgnoreCoalesce(nonHidden);
                    sb.Append(QuickSearchHelper.QuickSearchStatement(queryAttribute, entityAssociation.DbType, ignoreCoalesce));
                }
                if (!first) {
                    //at least one entry was found, closing parenthesis
                    sb.Append(")");
                }

            }
            return sb.ToString();
        }

        private static bool IgnoreCoalesce(ApplicationFieldDefinition fieldDefinition) {
            return fieldDefinition.DataType != null && fieldDefinition.DataType.Equals("text") && !fieldDefinition.DeclaredAsQueryOnEntity;
        }

        internal static string AttribteAppendingApplicationPrefix(ApplicationSchemaDefinition schema, string attribute, EntityMetadata entity, IList<ApplicationFieldDefinition> displayables) {

            var attributes = entity.Attributes(EntityMetadata.AttributesMode.NoCollections);
            var result = entity.LocateNonCollectionAttribute(attribute, attributes);

            if (result == null && attribute.StartsWith("#")) {
                var field = displayables.FirstOrDefault(d => d.Attribute.Equals(attribute));
                if (field?.AttributeToServer != null) {
                    attribute = field.AttributeToServer;
                }
            }


            if (result?.Item1.Query != null) {
                return AssociationHelper.PrecompiledAssociationAttributeQuery(entity.Name, result.Item1);
            }
            if (attribute.Contains(".")) {
                return attribute;
            }
            //this is used to avoid duplications between multiple parameters
            return entity.Name + "." + attribute;

        }
    }
}
