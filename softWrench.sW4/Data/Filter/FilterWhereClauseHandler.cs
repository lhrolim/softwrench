using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.Shared2.Metadata.Applications.Filter;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;

namespace softWrench.sW4.Data.Filter {

    public class FilterWhereClauseHandler : ISingletonComponent {
        public PaginatedSearchRequestDto HandleDTO(ApplicationSchemaDefinition schema, PaginatedSearchRequestDto searchDto) {
            //force cache here
            var parameters = searchDto.GetParameters();
            var schemaFilters = schema.SchemaFilters;
            if (!schemaFilters.HasOverridenFilter || parameters == null) {
                //if all filters are the column filters no need to take any action
                return searchDto;
            }

            var entity = MetadataProvider.EntityByApplication(schema.ApplicationName);

            var whereClauseFilters = schemaFilters.Filters.Where(f => f.WhereClause != null);


            foreach (var whereClauseFilter in whereClauseFilters) {
                SearchParameter paramValue = searchDto.RemoveSearchParam(whereClauseFilter.Attribute);
                if (paramValue == null) {
                    //this has not came from the client side as a client filter
                    continue;
                }

                var whereClause = whereClauseFilter.WhereClause;

                if (whereClauseFilter is MetadataOptionFilter && paramValue.SearchOperator.Equals(SearchOperator.CONTAINS) && !whereClauseFilter.IsTransient()) {
                    paramValue.IgnoreParameter = false;
                    //this means that we´re using a contains operation inside of an option filter, which should lead to default attribute lookup
                    continue;
                }

                if (!whereClause.StartsWith("@")) {
                    var values = paramValue.Value as IEnumerable<string>;
                    if (values != null) {
                        whereClause = whereClause.Replace("!@#value", BaseQueryUtil.GenerateInString(values));
                    } else if (paramValue.Value is string) {
                        whereClause = whereClause.Replace("!@#value", "'" + paramValue.Value + "'");
                    }
                    whereClause = whereClause.Replace("!@#value", paramValue.Value as string);
                    whereClause = whereClause.Replace("!@", entity.Name + ".");
                    //vanilla string case
                    searchDto.AppendWhereClause(whereClause);

                }
            }

            return searchDto;
        }
    }
}
