using System.Linq;
using cts.commons.simpleinjector;
using softwrench.sw4.Shared2.Metadata.Applications.Filter;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;

namespace softWrench.sW4.Data.Search {
    public class QuickSearchWhereClauseHandler : ISingletonComponent {

        private const string QUICK_SEARCH_PARAM_QUERY_PATTERN = "({0} like :{0})";
        private const string QUICK_SEARCH_PARAM_VALUE_PATTERN = "%{0}%";

        /// <summary>
        /// Appends whereclause and it's query parameter on the dto for a quick search 
        /// ('or' and like in all declared filters's attributes).
        /// The query parameter will have the value of the dto's QuickSearchData property.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public PaginatedSearchRequestDto HandleDTO(ApplicationSchemaDefinition schema, PaginatedSearchRequestDto dto) {
            if (dto == null || string.IsNullOrEmpty(dto.QuickSearchData)) return dto;
            var searchData = dto.QuickSearchData;
            // iterate filters and 'OR' the attributes
            var whereClause = string.Join("OR",
                schema.SchemaFilters.Filters
                    // filter out datetime and boolean filters
                    .Where(f => !(f is MetadataBooleanFilter) && !(f is MetadataDateTimeFilter))
                    // statement named parameter with value of QuickSearch string
                    .Select(f => {
                        dto.AppendSearchEntry(f.Attribute, string.Format(QUICK_SEARCH_PARAM_VALUE_PATTERN, searchData));
                        return f;
                    })
                    // build the attribute statement
                    .Select(f => string.Format(QUICK_SEARCH_PARAM_QUERY_PATTERN, f.Attribute)));
            dto.AppendWhereClause(whereClause);
            return dto;
        }

    }
}
