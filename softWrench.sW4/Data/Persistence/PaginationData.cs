using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;

namespace softWrench.sW4.Data.Persistence {
    public class PaginationData {
        public int PageSize = 0;
        public int PageNumber = 1;

        public string SortString { get; set; }

        private PaginationData(int pageSize, int pageNumber, string sortString) {
            PageSize = pageSize;
            PageNumber = pageNumber;
            SortString = sortString;
        }

        public static PaginationData GetInstance(SearchRequestDto searchDTO, EntityMetadata entityMetadata) {
            var paginatedSearchRequestDto = searchDTO as PaginatedSearchRequestDto;
            PaginationData paginationData = null;
            if (paginatedSearchRequestDto != null && paginatedSearchRequestDto.PageSize > 0 && paginatedSearchRequestDto.ShouldPaginate) {
                paginationData = new PaginationData(paginatedSearchRequestDto.PageSize,
                                                                     paginatedSearchRequestDto.PageNumber, QuerySearchSortBuilder.BuildSearchSort(entityMetadata, searchDTO));
            }
            return paginationData;
        }


    }
}
