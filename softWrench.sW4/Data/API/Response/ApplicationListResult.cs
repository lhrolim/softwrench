using System.Collections.Generic;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.Pagination;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Association.SchemaLoading;

namespace softWrench.sW4.Data.API.Response {
    public class ApplicationListResult : GenericResponseResult<IEnumerable<AttributeHolder>>, IApplicationResponse {
        public PaginatedSearchRequestDto PageResultDto;
        private string _mode;
        //this is for grids that have optionfields inside of it

        public ApplicationListResult(int totalCount, PaginatedSearchRequestDto searchDTO,
            IEnumerable<AttributeHolder> dataMap, ApplicationSchemaDefinition schema, AssociationMainSchemaLoadResult associationOptions)
            : base(dataMap, null) {
            Schema = schema;
            PageResultDto = new PaginatedSearchRequestDto(totalCount, searchDTO.PageNumber, searchDTO.PageSize, searchDTO.SearchValues, searchDTO.PaginationOptions);
            PageResultDto.SearchParams = searchDTO.SearchParams;
            PageResultDto.FilterFixedWhereClause = searchDTO.FilterFixedWhereClause;
            AssociationOptions = associationOptions;
        }

        public ApplicationSchemaDefinition Schema { get; set; }
        public string CachedSchemaId { get; set; }

        public IEnumerable<UserProfile.UserProfileDTO> AffectedProfiles {get; set;}

        public int? CurrentSelectedProfile {get; set;}

        public AssociationMainSchemaLoadResult AssociationOptions { get; set; }

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

        public string FilterFixedWhereClause { get { return PageResultDto.FilterFixedWhereClause; } }


        public IEnumerable<PageToShow> PagesToShow { get { return PageResultDto.PagesToShow; } }

        #endregion
    }
}
