using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Search.QuickSearch;

namespace softWrench.sW4.Data.Filter {
    public class FilterDTOHandlerComposite {

        [Import]
        public FilterWhereClauseHandler FilterWhereClauseHandler {
            get; set;
        }

        [Import]
        public QuickSearchWhereClauseHandler QuickSearchWhereClauseHandler {
            get; set;
        }

        [Import]
        public SortHandler SortHandler {
            get; set;
        }

        public void HandleDTO(ApplicationSchemaDefinition schema, PaginatedSearchRequestDto searchDto) {
            FilterWhereClauseHandler.HandleDTO(schema, searchDto);
            QuickSearchWhereClauseHandler.HandleDTO(schema, searchDto);
            SortHandler.HandleSearchDTO(schema, searchDto);

        }

    }
}
