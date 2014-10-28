using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {
    class BaseDataSetSearchHelper {

        internal static PaginatedSearchRequestDto BuildSearchDTOForAssociationSearch(AssociationUpdateRequest request, ApplicationAssociationDefinition association, AttributeHolder cruddata) {

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
            } else {
                if (!String.IsNullOrWhiteSpace(valueSearchString)) {
                    searchRequest.AppendSearchParam(association.EntityAssociation.PrimaryAttribute().To);
                    searchRequest.AppendSearchValue("%" + valueSearchString + "%");
                }
                if (!String.IsNullOrWhiteSpace(request.LabelSearchString)) {
                    AppendSearchLabelString(request, association, searchRequest);
                }
            }
            return searchRequest;
        }

        /// <summary>
        ///  this is used for both autocompleteserver or lookup to peform the search on the server based upon the labe string
        /// </summary>
        /// <param name="request"></param>
        /// <param name="association"></param>
        /// <param name="searchRequest"></param>
        private static void AppendSearchLabelString(AssociationUpdateRequest request,
            ApplicationAssociationDefinition association, PaginatedSearchRequestDto searchRequest) {
            var sbParam = new StringBuilder("(");
            var sbValue = new StringBuilder();

            foreach (var labelField in association.LabelFields) {
                sbParam.Append(labelField).Append(SearchUtils.SearchParamOrSeparator);
                sbValue.Append("%" + request.LabelSearchString + "%").Append(SearchUtils.SearchValueSeparator);
            }

            sbParam.Remove(sbParam.Length - SearchUtils.SearchParamOrSeparator.Length, SearchUtils.SearchParamOrSeparator.Length);
            sbValue.Remove(sbValue.Length - SearchUtils.SearchValueSeparator.Length, SearchUtils.SearchValueSeparator.Length);
            sbParam.Append(")");
            searchRequest.AppendSearchEntry(sbParam.ToString(), sbValue.ToString());
        }

    }
}
