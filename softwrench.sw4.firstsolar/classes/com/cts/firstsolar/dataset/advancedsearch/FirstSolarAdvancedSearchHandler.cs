using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch {
    public class FirstSolarAdvancedSearchHandler : ISingletonComponent {
        private const string FsLocSearchAttribute = "fslocationsearch";
        private const string FsLocSearchIncludeSublocAttribute = "fsincludesubloc";
        private const string FsLocSearchFacilityAttribute = "fsfacility";
        private const string FsLocSearchLocOfInterestAttribute = "fslocint";
        private const string FsLocSearchSwitchgearsAttribute = "fsswitchgear";
        private const string FsLocSearchPcsFormAttribute = "pcs_";
        private const string FsLocSearchBlockAttribute = "fsblock";
        private const string FsLocSearchPcsAttribute = "fspcs";

        private readonly FirstSolarBaseLocationFinder _baseLocationFinder;
        private readonly FirstSolarAdvancedSearchPcsHandler _advancedSearchPcsHandler;

        public FirstSolarAdvancedSearchHandler(FirstSolarBaseLocationFinder baseLocationFinder, FirstSolarAdvancedSearchPcsHandler advancedSearchPcsHandler) {
            _baseLocationFinder = baseLocationFinder;
            _advancedSearchPcsHandler = advancedSearchPcsHandler;
        }

        public bool IsAdvancedSearch(PaginatedSearchRequestDto searchDto) {
            var parameters = searchDto.GetParameters();
            return parameters != null && parameters.Any(p => FsLocSearchAttribute.Equals(p.Key));
        }

        public void AppendAdvancedSearchWhereClause(ApplicationMetadata application, PaginatedSearchRequestDto searchDto, string tableName) {
            var advancedSearchClause = new StringBuilder();
            searchDto.RemoveSearchParam(FsLocSearchAttribute);
            searchDto.RemoveSearchParam(FsLocSearchPcsFormAttribute);
            var facility = searchDto.RemoveSearchParam(FsLocSearchFacilityAttribute);

            var facilityList = facility.Value as List<string>;
            var facilityString = facility.Value as string;
            if (!string.IsNullOrEmpty(facilityString)) {
                facilityList = new List<string> { facilityString };
            }

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
            var blockList = block.Value as List<string>;
            var missingBlock = string.IsNullOrEmpty(blockString) && blockList == null;

            var pcsString = pcs.Value as string;
            var pcsList = pcs.Value as List<string>;
            var missingPcs = string.IsNullOrEmpty(pcsString) && pcsList == null;

            if (missingBlock && missingPcs) {
                searchDto.AppendWhereClause(advancedSearchClause.ToString());
                return;
            }
            if (blockList == null) {
                blockList = new List<string> { blockString };
            }
            if (pcsList == null) {
                pcsList = new List<string> { pcsString };
            }

            var baseLocations = _advancedSearchPcsHandler.GetBaseLocations(facilityList, blockList, pcsList);
            var pcsLocationsClause = BuildAdvancedSearchWhereClause(baseLocations, tableName, includeSubloc);
            AppendWhereClause(advancedSearchClause, pcsLocationsClause);

            if (advancedSearchClause.Length == 0) {
                // forces no results
                searchDto.AppendWhereClause("1=0");
                return;
            }
            searchDto.AppendWhereClause(advancedSearchClause.ToString());
        }

        public List<Dictionary<string, string>> GetLocationsOfInterest(List<string> facilities) {
            var baseLikes = new List<string> { "-%-00" };
            return _baseLocationFinder.FindBaseLocations(facilities, baseLikes);
        }

        public List<Dictionary<string, string>> GetSwitchgearLocations(List<string> facilities) {
            var baseLikes = new List<string> { "-%-%-00" };
            return _baseLocationFinder.FindBaseLocations(facilities, baseLikes);
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
    }
}
