using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using NHibernate.Linq;
using softwrench.sw4.Shared2.Metadata.Applications.Filter;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;

namespace softWrench.sW4.Data.Search {
    public class QuickSearchWhereClauseHandler : ISingletonComponent {

        /// <summary>
        /// Appends whereclause and it's query parameter on the dto for a quick search 
        /// ('or' and like in all declared filters's attributes).
        /// The query parameter will have the value of the dto's QuickSearchData property.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public PaginatedSearchRequestDto HandleDTO(ApplicationSchemaDefinition schema, PaginatedSearchRequestDto dto) {
            if (dto == null || !QuickSearchHelper.HasQuickSearchData(dto)) return dto;
            // iterate filters and 'OR' the attributes

            var entity = MetadataProvider.Entity(schema.EntityName);
            //caching this call
            var attributes = entity.Attributes(EntityMetadata.AttributesMode.NoCollections);

            var validFilterAttributes = schema.SchemaFilters.Filters
                // filter out datetime and boolean filters
                .Where(f => !(f is MetadataBooleanFilter) && !(f is MetadataDateTimeFilter))
                .Select(f => AttribteAppendingApplicationPrefix(f.Attribute, entity, attributes));

            var whereClause = QuickSearchHelper.BuildOrWhereClause(validFilterAttributes);

            // appending just where clause statement: value of the statement parameter is set at SearchUtils#GetParameters
            dto.AppendWhereClause(whereClause);
            return dto;
        }

        private static string AttribteAppendingApplicationPrefix(string attribute, EntityMetadata entity, IEnumerable<EntityAttribute> attributes) {

            var result = entity.LocateNonCollectionAttribute(attribute, attributes);
            if (result.Item1.Query != null) {
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
