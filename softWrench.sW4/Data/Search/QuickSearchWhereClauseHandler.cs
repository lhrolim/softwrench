using System.Linq;
using cts.commons.simpleinjector;
using softwrench.sw4.Shared2.Metadata.Applications.Filter;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;

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
            var whereClause = string.Join("OR",
                schema.SchemaFilters.Filters
                    // filter out datetime and boolean filters
                    .Where(f => !(f is MetadataBooleanFilter) && !(f is MetadataDateTimeFilter))
                    // build the attribute statement
                    .Select(f => QuickSearchHelper.QuickSearchStatement(f.Attribute)));

            // appending just where clause statement: value of the statement parameter is set at SearchUtils#GetParameters
            dto.AppendWhereClause(whereClause);
            return dto;
        }

        

    }
}
