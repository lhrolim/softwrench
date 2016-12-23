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

        //TODO: rethink of this schema, shouldn't be needed to pass each time

        public LookupOptionsFetchResultDTO(IEnumerable<IAssociationOption> associationData, int defaultPageSize, List<int> paginationOptions) : base(associationData)
        {
            _pageResultDto = new PaginatedSearchRequestDto(defaultPageSize, paginationOptions) {
                TotalCount = associationData?.Count() ?? 0
            };
        }

        public LookupOptionsFetchResultDTO(int totalCount, int pageNumber, int pageSize, IEnumerable<IAssociationOption> associationData, ApplicationMetadata associationApplicationMetadata, SearchRequestDto searchDTO = null)
            : base(associationData) {
            _pageResultDto = new PaginatedSearchRequestDto(totalCount, pageNumber, pageSize, null, PaginatedSearchRequestDto.DefaultPaginationOptions);
            SearchDTO = searchDTO;

            if (associationApplicationMetadata != null)
            {
                AssociationSchemaDefinition = associationApplicationMetadata.Schema;
            }
        }

        public ApplicationSchemaDefinition AssociationSchemaDefinition { get; }

        #region PagingDelegateMethods

        public int TotalCount => _pageResultDto.TotalCount;

        public int PageNumber => _pageResultDto.PageNumber;

        public int PageSize => _pageResultDto.PageSize;

        public int PageCount => _pageResultDto.PageCount;

        public IEnumerable<PageToShow> PagesToShow => _pageResultDto.PagesToShow;

        public SearchRequestDto SearchDTO { get; }

        #endregion
    }
}
