using JetBrains.Annotations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Metadata.Applications.DataSet.Filter {
    public class FilterWhereClauseParameters {

        public ApplicationSchemaDefinition Schema {
            get; private set;
        }
        public PaginatedSearchRequestDto SearchDto {
            get; private set;
        }
        public string InputString {
            get; private set;
        }

        public SearchParameter Param {
            get; set;
        }

        public FilterWhereClauseParameters(ApplicationSchemaDefinition schema, PaginatedSearchRequestDto searchDto, [NotNull]SearchParameter param) {
            Schema = schema;
            SearchDto = searchDto;
            if (param.Value != null) {
                InputString = param.Value.ToString();
            }
            Param = param;
        }


    }
}
