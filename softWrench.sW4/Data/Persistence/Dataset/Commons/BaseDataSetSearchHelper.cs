using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softWrench.sW4.Data.Search.QuickSearch;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {
    public class BaseDataSetSearchHelper : ISingletonComponent {

        private readonly QuickSearchHelper _quickSearchHelper;

        public BaseDataSetSearchHelper(QuickSearchHelper quickSearchHelper) {
            _quickSearchHelper = quickSearchHelper;
        }

        /// <summary>
        /// Main method for creating the searchDTO to search a given association 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="association"></param>
        /// <param name="cruddata"></param>
        /// <returns></returns>
        //TODO: rethink this method
        public PaginatedSearchRequestDto BuildSearchDTOForAssociationSearch(AssociationUpdateRequest request,
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
            } else if (request.SearchDTO.QuickSearchDTO != null) {
                searchRequest.AppendWhereClause(_quickSearchHelper.BuildOrWhereClause(new List<string>{
                    association.EntityAssociation.PrimaryAttribute().To,
                    association.LabelFields.FirstOrDefault(),
                }));

            }

            return searchRequest;
        }



    }
}
