using System;
using System.Collections.Generic;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.API {
    public class ApplicationListResult : GenericResponseResult<IEnumerable<AttributeHolder>>, IApplicationResponse {

        private readonly ApplicationSchemaDefinition _schema;
        public PaginatedSearchRequestDto PageResultDto;
        private string _mode;

        public ApplicationListResult(int totalCount, PaginatedSearchRequestDto searchDTO,
            IEnumerable<AttributeHolder> dataMap, ApplicationSchemaDefinition schema)
            : base(dataMap, null) {
            _schema = schema;
            PageResultDto = new PaginatedSearchRequestDto(totalCount, searchDTO.PageNumber, searchDTO.PageSize, searchDTO.SearchValues, searchDTO.PaginationOptions);
            PageResultDto.SearchParams = searchDTO.SearchParams;
            PageResultDto.FilterFixedWhereClause = searchDTO.FilterFixedWhereClause;
            PageResultDto.UnionFilterFixedWhereClause = searchDTO.UnionFilterFixedWhereClause;
        }

        public ApplicationSchemaDefinition Schema {
            get { return _schema; }
        }

        public string Mode {
            get { return Schema.Mode.ToString().ToLower(); }
            set { _mode = value; }
        }

        public string ApplicationName { get { return Schema.ApplicationName; } }

        public string Type {
            get { return GetType().Name; }
        }

        #region PagingDelegateMethods

        public int TotalCount { get { return PageResultDto.TotalCount; } }

        public int PageNumber { get { return PageResultDto.PageNumber; } }

        public int PageSize { get { return PageResultDto.PageSize; } }

        public List<int> PaginationOptions { get { return PageResultDto.PaginationOptions; } }

        public string SearchValues { get { return PageResultDto.SearchValues; } }

        public int PageCount { get { return PageResultDto.PageCount; } }

        public String FilterFixedWhereClause { get { return PageResultDto.FilterFixedWhereClause; } }

        public String UnionFilterFixedWhereClause { get { return PageResultDto.UnionFilterFixedWhereClause; } }


        public IEnumerable<PageToShow> PagesToShow { get { return PageResultDto.PagesToShow; } }

        #endregion
    }
}
