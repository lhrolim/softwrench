using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.api.classes.fwk.filter;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {

    public class FirstSolarPCSLocationHandler : ISingletonComponent {

        private readonly EntityRepository _entityRepository;

        public FirstSolarPCSLocationHandler(EntityRepository entityRepository) {
            _entityRepository = entityRepository;
        }

        public string HandlePCSLocation(FilterWhereClauseParameters whereClauseParameters) {
            var inputSearch = whereClauseParameters.InputString;
            whereClauseParameters.Param.IgnoreParameter = false;
            //this should lead to default filter implementation as well
            var pCsQueryString = DoGetPCSQuery(inputSearch, whereClauseParameters.Schema.EntityName);
            if (inputSearch.GetNumberOfItems("%") == 0) {
                HandleSubLocationQueries(whereClauseParameters);
            }
            return pCsQueryString;
        }

        public IEnumerable<IAssociationOption> HandlePCSLocationProvider(FilterProviderParameters filterParameter) {
            var inputSearch = filterParameter.InputSearch;
            var pCsQueryString = DoGetPCSQuery(filterParameter.InputSearch, "Location");
            if (inputSearch.GetNumberOfItems("%") == 0) {
                filterParameter.InputSearch = inputSearch + "%";
            }
            var filter = new PaginatedSearchRequestDto();

            filter.AppendWhereClause(pCsQueryString);
            filter.AppendWhereClause("(location like '{0}' or description like '{0}' )".Fmt(filterParameter.InputSearch));
            filter.AppendProjectionField(ProjectionField.Default("location"));
            filter.AppendProjectionField(ProjectionField.Default("description"));
            var location = MetadataProvider.Entity("location");
            //let´s limit the filter adding an extra value so that we know there´re more to be brought
            //TODO: add a count call
            //            if (!association.EntityAssociation.Cacheable) {
            filter.PageSize = 21;
            //            }
            //adopting to use an association to keep same existing service
            var queryResponse = _entityRepository.Get(location, filter);

            ISet<IAssociationOption> options = new SortedSet<IAssociationOption>();
            foreach (var attributeHolder1 in queryResponse) {
                var attributeHolder = (DataMap)attributeHolder1;
                var value = attributeHolder.GetAttribute("location");
                var label = attributeHolder.GetAttribute("description") as string;
                // If the value is null, skip this conversion and continue executing
                if (value == null) {
                    continue;
                }
                options.Add(new AssociationOption(Convert.ToString(value), label));
            }
            return options;


        }

        private static void HandleSubLocationQueries(FilterWhereClauseParameters whereClauseParameters) {

            var filterValue = whereClauseParameters.Param.Value;

            var list = filterValue as IEnumerable<string>;
            if (list == null) {
                //string scenario
                //appending a "%" if the 
                whereClauseParameters.Param.Refresh(((string)whereClauseParameters.Param.Value) + "%");
            } else {
                var newList = new List<string>();
                foreach (var item in list) {
                    newList.Add(item + "%");
                }
                whereClauseParameters.Param.Refresh(string.Join(",", newList));
            }



        }

        public string DoGetPCSQuery(string inputSearch, string tableName) {
            if (string.IsNullOrEmpty(inputSearch)) {
                return null;
            }


            if (inputSearch.GetNumberOfItems("%") > 2 && inputSearch.StartsWith("%") && inputSearch.EndsWith("%")) {
                //removing leading and trailing wyldcards. This is due to the fact that the framework is appending it, automatically on contain searchs on client side.
                //decided to threat it here, just for firstsolar
                inputSearch = inputSearch.Substring(1);
                inputSearch = inputSearch.Substring(0, inputSearch.Length - 1);
            }


            if (inputSearch.EndsWith("%") || !inputSearch.Contains("%")) {
                //apply default sublocation search
                return null;
            }

            var numberOfDashes = inputSearch.GetNumberOfItems("-");
            if (numberOfDashes != 4) {
                //apply default search to these cases
                return null;
            }


            return "LEN({0}.location) - LEN(REPLACE({0}.location, '-', '')) = 4".Fmt(tableName);
        }
    }
}
