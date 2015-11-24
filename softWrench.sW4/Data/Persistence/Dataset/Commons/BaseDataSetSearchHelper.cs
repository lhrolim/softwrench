using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {
    class BaseDataSetSearchHelper {

        internal static PaginatedSearchRequestDto BuildSearchDTOForAssociationSearch(AssociationUpdateRequest request,
            ApplicationAssociationDefinition association, AttributeHolder cruddata) {

            var searchRequest = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);

            if (request.SearchDTO == null) {
                request.SearchDTO = PaginatedSearchRequestDto.DefaultInstance(null);
            }
            searchRequest.PageNumber = request.SearchDTO.PageNumber;
            searchRequest.PageSize = request.SearchDTO.PageSize;
            //avoids pagination unless the association renderer defines so (lookup)
            searchRequest.ShouldPaginate = association.IsPaginated();
            searchRequest.NeedsCountUpdate = association.IsPaginated();
            var valueSearchString = request.ValueSearchString;
            if (association.IsLazyLoaded() && !request.HasClientSearch) {
                if ((cruddata == null || cruddata.GetAttribute(association.Target) == null)) {
                    //we should not update lazy dependant associations except in one case:
                    //there´s a default value in place already for the dependent association
                    // in that case, we would need to return a 1-value list to show on screen
                    return null;
                }
                //this will force that the search would be made only on that specific value
                //ex: autocomplete server, lookups that depend upon another association
                valueSearchString = cruddata.GetAttribute(association.Target) as string;
            }

            if (request.AssociationKey != null) {
                // If association has a schema key defined, the searchDTO will be filled on client, so just copy it from request
                searchRequest.SearchParams = request.SearchDTO.SearchParams;
                searchRequest.SearchValues = request.SearchDTO.SearchValues;
            } else if (!string.IsNullOrEmpty(request.SearchDTO.QuickSearchData)) {
                searchRequest.AppendWhereClause(QuickSearchHelper.BuildOrWhereClause(new List<string>{
                    association.EntityAssociation.PrimaryAttribute().To,
                    association.LabelFields.FirstOrDefault(),
                }));

            }

            return searchRequest;
        }

      

    }
}
