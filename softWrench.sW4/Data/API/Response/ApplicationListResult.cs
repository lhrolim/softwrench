using System;
using System.Collections.Generic;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.Pagination;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.API.Response {
    public class ApplicationListResult : GenericResponseResult<IEnumerable<AttributeHolder>>, IApplicationResponse {

        private readonly ApplicationSchemaDefinition _schema;
        public PaginatedSearchRequestDto PageResultDto;
        private string _mode;
        //this is for grids that have optionfields inside of it
        private IDictionary<string, BaseAssociationUpdateResult> _associationOptions = new Dictionary<string, BaseAssociationUpdateResult>();

        public ApplicationListResult(int totalCount, PaginatedSearchRequestDto searchDTO,
            IEnumerable<AttributeHolder> dataMap, ApplicationSchemaDefinition schema, IDictionary<string, BaseAssociationUpdateResult> associationOptions)
            : base(dataMap, null) {
            _schema = schema;
            PageResultDto = new PaginatedSearchRequestDto(totalCount, searchDTO.PageNumber, searchDTO.PageSize, searchDTO.SearchValues, searchDTO.PaginationOptions);
            PageResultDto.SearchParams = searchDTO.SearchParams;
            PageResultDto.FilterFixedWhereClause = searchDTO.FilterFixedWhereClause;
            _associationOptions = associationOptions;
        }

        public ApplicationSchemaDefinition Schema {
            get { return _schema; }
        }

        public IDictionary<string, BaseAssociationUpdateResult> AssociationOptions {
            get { return _associationOptions; }
            set { _associationOptions = value; }
        }

        public string Mode {
            get { return Schema.Mode.ToString().ToLower(); }
            set { _mode = value; }
        }

        public string ApplicationName { get { return Schema.ApplicationName; } }
        public string Id { get; private set; }

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


        public IEnumerable<PageToShow> PagesToShow { get { return PageResultDto.PagesToShow; } }

        #endregion
    }
}
