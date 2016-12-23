using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.api.classes.integration;
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
            if (searchDTO == null) {
                searchDTO = PaginatedSearchRequestDto.DefaultInstance(schema);
            }
            PageResultDto = new PaginatedSearchRequestDto(totalCount, searchDTO.PageNumber, searchDTO.PageSize, searchDTO.SearchValues, searchDTO.PaginationOptions) {
                SearchParams = searchDTO.SearchParams,
                FilterFixedWhereClause = searchDTO.FilterFixedWhereClause,
                QuickSearchDTO = searchDTO.QuickSearchDTO,
                SearchTemplate = searchDTO.SearchTemplate,
                SearchAscending = searchDTO.SearchAscending,
                SearchSort = searchDTO.SearchSort,
                MultiSearchSort = searchDTO.MultiSearchSort
            };
            AssociationOptions = associationOptions;
        }

        public static ApplicationListResult FixedListResult(IEnumerable<AttributeHolder> dataMap, ApplicationSchemaDefinition schema) {
            var attributeHolders = dataMap as AttributeHolder[] ?? dataMap.ToArray();
            return new ApplicationListResult(attributeHolders.Count(), null, attributeHolders, schema, null);
        }


        public ApplicationSchemaDefinition Schema {
            get; set;
        }
        public string CachedSchemaId {
            get; set;
        }

        public IEnumerable<UserProfile.UserProfileDTO> AffectedProfiles {
            get; set;
        }

        public int? CurrentSelectedProfile {
            get; set;
        }

        public AssociationMainSchemaLoadResult AssociationOptions {
            get; set;
        }

        public string Mode {
            get {
                return Schema.Mode.ToString().ToLower();
            }
            set {
                _mode = value;
            }
        }

        public string ApplicationName => Schema.ApplicationName;

        public string Id {
            get; private set;
        }

        public IErrorDto WarningDto {
            get; set;
        }


        public bool ShouldSerializeSchema() {
            return (CachedSchemaId == null);
        }

        #region PagingDelegateMethods

        public int TotalCount {
            get {
                return PageResultDto.TotalCount;
            }
        }

        public int PageNumber {
            get {
                return PageResultDto.PageNumber;
            }
        }

        public int PageSize {
            get {
                return PageResultDto.PageSize;
            }
        }

        public List<int> PaginationOptions {
            get {
                return PageResultDto.PaginationOptions;
            }
        }

        public string SearchValues {
            get {
                return PageResultDto.SearchValues;
            }
        }

        public int PageCount {
            get {
                return PageResultDto.PageCount;
            }
        }

        public string FilterFixedWhereClause {
            get {
                return PageResultDto.FilterFixedWhereClause;
            }
        }


        public IEnumerable<PageToShow> PagesToShow {
            get {
                return PageResultDto.PagesToShow;
            }
        }

        #endregion
    }
}
