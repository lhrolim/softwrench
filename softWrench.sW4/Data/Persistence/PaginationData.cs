﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;

namespace softWrench.sW4.Data.Persistence {
    public class PaginationData {
        public int PageSize = 0;
        public int PageNumber = 1;
        //this is needed because nhibernate builds buggy pagination queries, not appending the the alias on the over query. 
        //in some scenarios this may lead to ambigous column definition
        public string QualifiedOrderByColumn { get; set; }
        public string OrderByColumn { get; set; }

        public PaginationData(int pageSize, int pageNumber, string qualifiedSortColumn) {
            PageSize = pageSize;
            PageNumber = pageNumber;
            QualifiedOrderByColumn = qualifiedSortColumn;
            OrderByColumn = qualifiedSortColumn.Split('.')[1];
        }

        public static PaginationData GetInstance(SearchRequestDto searchDTO, EntityMetadata entityMetadata) {
            var qualifiedOrderColumn = entityMetadata.Name + "." + entityMetadata.IdFieldName;
            var searchSort = searchDTO.SearchSort;
            var suffix = searchDTO.SearchAscending ? " asc" : " desc";
            if (searchSort != null) {
                if (searchSort.IndexOf('.') == -1) {
                    //prepending the entity name on it
                    qualifiedOrderColumn = entityMetadata.Name + "." + searchSort;
                } else {
                    qualifiedOrderColumn = searchSort;
                }
            }
            if (!qualifiedOrderColumn.EndsWith("asc") && !qualifiedOrderColumn.EndsWith("desc")) {
                qualifiedOrderColumn += suffix;
            }
            var paginatedSearchRequestDto = searchDTO as PaginatedSearchRequestDto;
            PaginationData paginationData = null;
            if (paginatedSearchRequestDto != null && paginatedSearchRequestDto.PageSize > 0 && paginatedSearchRequestDto.ShouldPaginate) {
                paginationData = new PaginationData(paginatedSearchRequestDto.PageSize,
                                                                     paginatedSearchRequestDto.PageNumber, qualifiedOrderColumn);
            }
            return paginationData;
        }


    }
}
