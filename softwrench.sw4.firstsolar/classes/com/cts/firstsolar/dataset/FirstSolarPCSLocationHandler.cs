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
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {

    public class FirstSolarPCSLocationHandler : ISingletonComponent {

        private const string FsLocSearchAttribute = "fs_locationsearch";
        private const string FsLocSearchIncludeSublocAttribute = "fs_includesubloc";
        private const string FsLocSearchFacilityAttribute = "fs_facility";
        private const string FsLocSearchLocOfInterestAttribute = "fs_locint";
        private const string FsLocSearchSwitchgearsAttribute = "fs_switchgear";
        private const string FsLocSearchBlockAttribute = "fs_block";
        private const string FsLocSearchPcsAttribute = "fs_pcs";

        private const string BaseLocationQuery = "SELECT location, description FROM locations WHERE location LIKE ? AND LEN(locations.location) - LEN(REPLACE(locations.location, '-', '')) = ?";

        private readonly EntityRepository _entityRepository;
        private readonly IMaximoHibernateDAO _maximoHibernateDao;

        public FirstSolarPCSLocationHandler(EntityRepository entityRepository, IMaximoHibernateDAO maximoHibernateDao) {
            _entityRepository = entityRepository;
            _maximoHibernateDao = maximoHibernateDao;
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

        private static void AddBaseLocation(ICollection<string> baseLocationList, IReadOnlyDictionary<string, string> baseLocationRow) {
            var baseLocation = baseLocationRow["location"];
            if (baseLocation == null) {
                return;
            }
            baseLocationList.Add(baseLocation);
        }

        private List<string> GetBaseLocation(string baseLocationString) {
            var baseLocations = _maximoHibernateDao.FindByNativeQuery(BaseLocationQuery, baseLocationString, 4);
            var baseLocationList = new List<string>();
            baseLocations.ForEach(l => AddBaseLocation(baseLocationList, l));
            return baseLocationList;
        }

        private static void AddBaseLocationToClause(ref bool first, StringBuilder clause, string baseLocation, string tableName, bool includeSublocs) {
            if (!first) {
                clause.Append(" OR ");
            }
            first = false;
            clause.Append(tableName).Append(".location = '").Append(baseLocation).Append("' ");
            if (includeSublocs) {
                clause.Append(" OR ").Append(tableName).Append(".location LIKE '").Append(baseLocation).Append("-%' ");
            }
        }

        private static string BuildAdvancedSearchWhereClause(List<string> locations, string tableName, bool includeSublocs) {
            if (locations == null || locations.Count == 0) {
                return null;
            }
            var first = true;
            var locationsClause = new StringBuilder("(");
            locations.ForEach(l => AddBaseLocationToClause(ref first, locationsClause, l, tableName, includeSublocs));
            locationsClause.Append(")");
            return locationsClause.ToString();
        }

        private static string BuildAdvancedSearchWhereClause(SearchParameter sp, string tableName, bool includeSublocs) {
            if (sp == null || sp.Value == null) {
                return null;
            }

            var locList = sp.Value as List<string>;
            var locString = sp.Value as string;
            if (locString != null) {
                locList = new List<string>() { locString };
            }
            return BuildAdvancedSearchWhereClause(locList, tableName, includeSublocs);
        }

        private static void AppendWhereClause(StringBuilder advancedSearchClause, string whereClause) {
            if (string.IsNullOrEmpty(whereClause)) {
                return;
            }
            if (advancedSearchClause.Length != 0) {
                advancedSearchClause.Append(" OR ");
            }
            advancedSearchClause.Append(whereClause);
        }

        public bool IsAdvancedSearch(PaginatedSearchRequestDto searchDto) {
            var parameters = searchDto.GetParameters();
            return parameters != null && parameters.Any(p => FsLocSearchAttribute.Equals(p.Key));
        }

        public void AppendAdvancedSearchWhereClause(ApplicationMetadata application, PaginatedSearchRequestDto searchDto, string tableName) {
            var advancedSearchClause = new StringBuilder();
            searchDto.RemoveSearchParam(FsLocSearchAttribute);
            var facility = searchDto.RemoveSearchParam(FsLocSearchFacilityAttribute);

            var includeSublocSp = searchDto.RemoveSearchParam(FsLocSearchIncludeSublocAttribute);
            var includeSubloc = "TRUE".Equals(includeSublocSp.Value);

            var locOfInterestSp = searchDto.RemoveSearchParam(FsLocSearchLocOfInterestAttribute);
            var locOfInterestClause = BuildAdvancedSearchWhereClause(locOfInterestSp, tableName, includeSubloc);
            AppendWhereClause(advancedSearchClause, locOfInterestClause);

            var switchgearsSp = searchDto.RemoveSearchParam(FsLocSearchSwitchgearsAttribute);
            var switchgearsClause = BuildAdvancedSearchWhereClause(switchgearsSp, tableName, includeSubloc);
            AppendWhereClause(advancedSearchClause, switchgearsClause);

            var block = searchDto.RemoveSearchParam(FsLocSearchBlockAttribute);
            var pcs = searchDto.RemoveSearchParam(FsLocSearchPcsAttribute);

            if (block == null || pcs == null) {
                searchDto.AppendWhereClause(advancedSearchClause.ToString());
                return;
            }
            var blockString = block.Value as string;
            var pcsString = pcs.Value as string;
            if (string.IsNullOrEmpty(blockString) || string.IsNullOrEmpty(pcsString)) {
                searchDto.AppendWhereClause(advancedSearchClause.ToString());
                return;
            }

            var baseLocationSearchString = new StringBuilder();
            baseLocationSearchString.Append(facility.Value).Append("-");
            baseLocationSearchString.Append("%-").Append(blockString);
            baseLocationSearchString.Append("-%-").Append(pcsString);

            var baseLocations = GetBaseLocation(baseLocationSearchString.ToString());
            var pcsLocationsClause = BuildAdvancedSearchWhereClause(baseLocations, tableName, includeSubloc);
            AppendWhereClause(advancedSearchClause, pcsLocationsClause);

            if (advancedSearchClause.Length == 0) {
                // forces no results
                searchDto.AppendWhereClause("1=0");
                return;
            }
            searchDto.AppendWhereClause(advancedSearchClause.ToString());
        }

        public List<Dictionary<string, string>> GetLocationsOfInterest(string facility) {
            var locationString = string.Format("{0}-%-00", facility);
            var result = _maximoHibernateDao.FindByNativeQuery(BaseLocationQuery, locationString, 2);
            return result;
        }

        public List<Dictionary<string, string>> GetSwitchgearLocations(string facility) {
            var locationString = string.Format("{0}-%-%-00", facility);
            var result = _maximoHibernateDao.FindByNativeQuery(BaseLocationQuery, locationString, 3);
            return result;
        }
    }
}
