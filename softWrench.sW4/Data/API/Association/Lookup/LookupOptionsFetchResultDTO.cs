using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Data.API.Association.Lookup {
    public class LookupOptionsFetchResultDTO : BaseAssociationUpdateResult {

        private readonly PaginatedSearchRequestDto _pageResultDto;
        private readonly SearchRequestDto _searchDTO;

        //TODO: rethink of this schema, shouldn't be needed to pass each time
        private readonly ApplicationSchemaDefinition _associationSchemaDefinition;

        public LookupOptionsFetchResultDTO(IEnumerable<IAssociationOption> associationData, int defaultPageSize, List<int> paginationOptions) : base(associationData) {
            _pageResultDto = new PaginatedSearchRequestDto(defaultPageSize, paginationOptions) {
                TotalCount = associationData == null ? 0 : associationData.Count()
            };
        }

        public LookupOptionsFetchResultDTO(int totalCount, int pageNumber, int pageSize, IEnumerable<IAssociationOption> associationData, ApplicationMetadata associationApplicationMetadata, SearchRequestDto searchDTO = null)
            : base(associationData) {
            _pageResultDto = new PaginatedSearchRequestDto(totalCount, pageNumber, pageSize, null, PaginatedSearchRequestDto.DefaultPaginationOptions);
            _searchDTO = searchDTO;

            if (associationApplicationMetadata != null)
            {
                _associationSchemaDefinition = associationApplicationMetadata.Schema;
            }
        }

        public ApplicationSchemaDefinition AssociationSchemaDefinition { get { return _associationSchemaDefinition; } }


        #region PagingDelegateMethods

        public int TotalCount { get { return _pageResultDto.TotalCount; } }

        public int PageNumber { get { return _pageResultDto.PageNumber; } }

        public int PageSize { get { return _pageResultDto.PageSize; } }

        public int PageCount { get { return _pageResultDto.PageCount; } }

        public IEnumerable<PageToShow> PagesToShow { get { return _pageResultDto.PagesToShow; } }

        public SearchRequestDto SearchDTO { get { return _searchDTO; } }

        #endregion
    }
}
