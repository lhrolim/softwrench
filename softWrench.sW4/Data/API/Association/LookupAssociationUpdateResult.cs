using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata.Applications;
using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Data.API.Association {
    public class LookupAssociationUpdateResult : BaseAssociationUpdateResult {

        private readonly PaginatedSearchRequestDto _pageResultDto;
        private readonly ApplicationSchemaDefinition _associationSchemaDefinition;

        public LookupAssociationUpdateResult(IEnumerable<IAssociationOption> associationData, int defaultPageSize, List<int> paginationOptions)
            : base(associationData) {
            _pageResultDto = new PaginatedSearchRequestDto(defaultPageSize, paginationOptions) {
                TotalCount = associationData == null ? 0 : associationData.Count()
            };
        }

        public LookupAssociationUpdateResult(int totalCount, int pageNumber, int pageSize,
            IEnumerable<IAssociationOption> associationData, ApplicationMetadata associationMetadata, List<int> paginationOptions)
            : base(associationData) {
            _pageResultDto = new PaginatedSearchRequestDto(totalCount, pageNumber, pageSize, null, paginationOptions);
            if (associationMetadata != null) {
                _associationSchemaDefinition = associationMetadata.Schema;
            }
        }

        public ApplicationSchemaDefinition AssociationSchemaDefinition { get { return _associationSchemaDefinition; } }

        #region PagingDelegateMethods

        public int TotalCount { get { return _pageResultDto.TotalCount; } }

        public int PageNumber { get { return _pageResultDto.PageNumber; } }

        public int PageSize { get { return _pageResultDto.PageSize; } }

        public int PageCount { get { return _pageResultDto.PageCount; } }

        public IEnumerable<PageToShow> PagesToShow { get { return _pageResultDto.PagesToShow; } }

        #endregion
    }
}
