using System.Linq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data.Pagination {

    public class PaginatedSearchRequestDto : SearchRequestDto {

        private readonly int _pageCount;
        private bool _shouldPaginate = true;

        private readonly IList<PageToShow> _pagesToShow = new List<PageToShow>();

        private Boolean _needsCountUpdate = true;

//        private IList<String> _compositionsToFetch;

        public static List<int> DefaultPaginationOptions {
            get { return new List<int> { 10, 30, 100 }; }
        }


        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        /// <summary>
        /// Used for printing multiple pages
        /// </summary>
        public int NumberOfPages { get; set; }

        public int PageSize { get; set; }

        public List<int> PaginationOptions { get; set; }

        /// <summary>
        /// keep this flag so that the client can send always this class instead of sometimes this and sometimes SearchRequestDto,
        ///  as .net would be lost with json conversion.
        /// if the association is not defined, this should be false 
        /// </summary>
        public bool ShouldPaginate {
            get { return _shouldPaginate; }
            set { _shouldPaginate = value; }
        }

        public int PageCount {
            get { return _pageCount; }
        }

        public bool HasNext {
            get { return PageNumber != PageCount; }
        }

        public bool HasPrevious {
            get { return PageNumber != 1; }
        }

        public IList<PageToShow> PagesToShow {
            get { return _pagesToShow; }
        }

        /// <summary>
        /// by default, no compositions is fetched from the server on a list search
        /// if this property is not null, the compositions indicated on this list will
        /// be returned
        /// ex. detailed list search (for detailed print in grid)
        /// </summary>
        public IList<String> CompositionsToFetch { get; set; }

        public PaginatedSearchRequestDto()
            : this(30, DefaultPaginationOptions) {
        }


        public PaginatedSearchRequestDto(int defaultPageSize, List<int> paginationOptions) {
            PageNumber = 1;
            TotalCount = 0;
            PageSize = defaultPageSize;
            CompositionsToFetch = new List<string>();
            PaginationOptions = paginationOptions;
        }

        public PaginatedSearchRequestDto(int totalCount, int pageNumber, int pageSize, string searchValue, List<int> paginationOptions) {
            TotalCount = totalCount;
            PageSize = pageSize;
            _pageCount = CalculatePageCount(totalCount);
            PageNumber = pageNumber;
            SearchValues = searchValue;
            var limitTuple = PaginationUtils.GetPaginationBounds(pageNumber, _pageCount);
            for (int i = limitTuple.Item1; i <= limitTuple.Item2; i++) {
                _pagesToShow.Add(new PageToShow(i == pageNumber, i));
            }
            CompositionsToFetch = new List<string>();
            if (paginationOptions.Count == 1 && paginationOptions.First() == 0) {
                //TODO: fix on client side
                PaginationOptions = DefaultPaginationOptions;
            } else {
                PaginationOptions = paginationOptions;
            }
        }

        private int CalculatePageCount(int totalCount) {
            if (totalCount < PageSize || PageSize == 0) {
                return 1;
            }
            if (totalCount % PageSize == 0) {
                return totalCount / PageSize;
            }
            return (totalCount / PageSize) + 1;
        }

        public bool NeedsCountUpdate {
            get { return _needsCountUpdate; }
            set { _needsCountUpdate = value; }
        }

        public static PaginatedSearchRequestDto DefaultInstance(ApplicationSchemaDefinition schema) {
            var defaultSize = 30;
            var paginationOptions = DefaultPaginationOptions;
            if (schema != null) {
                var defaultSizeSt = schema.GetProperty(ApplicationSchemaPropertiesCatalog.DefaultPaginationSize);
                if (defaultSizeSt != null) {
                    defaultSize = Int32.Parse(defaultSizeSt);
                }
                var paginationOptionsSt = schema.GetProperty(ApplicationSchemaPropertiesCatalog.PaginationOptions);
                if (paginationOptionsSt != null) {
                    var paginationOptionsAux = paginationOptionsSt.Split(',');
                    var paginationOptionListAux = new List<int>();
                    foreach (var s in paginationOptionsAux) {
                        int option;
                        if (int.TryParse(s, out option)) {
                            paginationOptionListAux.Add(option);
                        }
                    }
                    if (paginationOptionListAux.Count > 0) {
                        paginationOptions = paginationOptionListAux;
                    }
                }
            }
            return new PaginatedSearchRequestDto(defaultSize, paginationOptions) { IsDefaultInstance = true };
        }

        public override SearchRequestDto ShallowCopy() {
            return (PaginatedSearchRequestDto)MemberwiseClone();
        }


    }
}
