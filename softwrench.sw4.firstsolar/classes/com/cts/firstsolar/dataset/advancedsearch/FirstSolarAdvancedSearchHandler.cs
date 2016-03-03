﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.simpleinjector;
using log4net;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch {
    public class FirstSolarAdvancedSearchHandler : ISingletonComponent {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarAdvancedSearchHandler));

        private const string FsLocSearchAttribute = "fslocationsearch";
        private const string FsLocSearchPcsFormAttribute = "pcs_";

        private readonly FirstSolarBaseLocationFinder _baseLocationFinder;
        private readonly FirstSolarAdvancedSearchPcsHandler _advancedSearchPcsHandler;

        public FirstSolarAdvancedSearchHandler(FirstSolarBaseLocationFinder baseLocationFinder, FirstSolarAdvancedSearchPcsHandler advancedSearchPcsHandler) {
            _baseLocationFinder = baseLocationFinder;
            _advancedSearchPcsHandler = advancedSearchPcsHandler;
        }

        /// <summary>
        /// If the current search is a first solar advanced search.
        /// </summary>
        /// <param name="searchDto"></param>
        /// <returns>True if the current search is a first solar advanced search.</returns>
        public bool IsAdvancedSearch(PaginatedSearchRequestDto searchDto) {
            var parameters = searchDto.GetParameters();
            return parameters != null && parameters.Any(p => FsLocSearchAttribute.Equals(p.Key));
        }

        public void AppendAdvancedSearchWhereClause(ApplicationMetadata application, PaginatedSearchRequestDto searchDto, string tableName) {
            var advancedSearchClause = new StringBuilder();
            searchDto.RemoveSearchParam(FsLocSearchAttribute);
            searchDto.RemoveSearchParam(FsLocSearchPcsFormAttribute);

            var facilityList = FirstSolarAdvancedSearchDtoUtils.GetFacilityList(searchDto);
            var includeSubloc = FirstSolarAdvancedSearchDtoUtils.GetIncludeSubLocations(searchDto);

            Log.Debug("Building locations of interest where clause...");
            var locOfInterest = FirstSolarAdvancedSearchDtoUtils.GetLocationsOfInterest(searchDto);
            var locOfInterestClause = BuildAdvancedSearchWhereClause(locOfInterest, tableName, includeSubloc);
            AppendWhereClause(advancedSearchClause, locOfInterestClause);

            Log.Debug("Building sitchgears locations where clause");
            var switchgears = FirstSolarAdvancedSearchDtoUtils.GetSwitchgears(searchDto);
            var switchgearsClause = BuildAdvancedSearchWhereClause(switchgears, tableName, includeSubloc);
            AppendWhereClause(advancedSearchClause, switchgearsClause);

            Log.Debug("Building pcs where clause");
            var blockList = FirstSolarAdvancedSearchDtoUtils.GetBlocks(searchDto);
            var pcsList = FirstSolarAdvancedSearchDtoUtils.GetPcsList(searchDto);
            var baseLocations = _advancedSearchPcsHandler.GetBaseLocations(facilityList, blockList, pcsList);
            var pcsLocationsClause = BuildAdvancedSearchWhereClause(baseLocations, tableName, includeSubloc);
            AppendWhereClause(advancedSearchClause, pcsLocationsClause);

            FinishAdvancedSearch(advancedSearchClause.ToString(), searchDto, facilityList, tableName);
        }

        /// <summary> 
        /// </summary>
        /// <param name="facilities">The list of selected facilities (right now one facility can represent a list).</param>
        /// <returns>The list of locations of interest.</returns>
        public List<Dictionary<string, string>> GetLocationsOfInterest(List<string> facilities) {
            var baseLikes = new List<string> { "-%-00" };
            return _baseLocationFinder.FindBaseLocations(facilities, baseLikes);
        }

        /// <summary> 
        /// </summary>
        /// <param name="facilities">The list of selected facilities (right now one facility can represent a list).</param>
        /// <returns>The list of switchgear locations.</returns>
        public List<Dictionary<string, string>> GetSwitchgearLocations(List<string> facilities) {
            var baseLikes = new List<string> { "-%-%-00" };
            return _baseLocationFinder.FindBaseLocations(facilities, baseLikes);
        }

        /// <summary>
        /// Builds a where clause based on a list of base locations.
        /// </summary>
        /// <param name="locations"></param>
        /// <param name="tableName"></param>
        /// <param name="includeSublocs"></param>
        /// <returns></returns>
        private static string BuildAdvancedSearchWhereClause(List<string> locations, string tableName, bool includeSublocs) {
            if (locations == null || !locations.Any()) {
                Log.Debug("Done: No locations => empty clause.");
                return null;
            }
            var first = true;
            var locationsClause = new StringBuilder("(");
            locations.ForEach(l => AddBaseLocationToClause(ref first, locationsClause, l, tableName, includeSublocs));
            locationsClause.Append(")");
            Log.Debug(string.Format("Done: {0}", locationsClause));
            return locationsClause.ToString();
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

        private static void AppendWhereClause(StringBuilder advancedSearchClause, string whereClause) {
            if (string.IsNullOrEmpty(whereClause)) {
                return;
            }
            if (advancedSearchClause.Length != 0) {
                advancedSearchClause.Append(" OR ");
            }
            advancedSearchClause.Append(whereClause);
        }

        private static void FinishAdvancedSearch(string advancedWhereClause, SearchRequestDto searchDto, List<string> facilityList, string tableName) {
            if (!string.IsNullOrEmpty(advancedWhereClause)) {
                Log.Debug("Full advanced search clause:");
                Log.Debug(advancedWhereClause);
                searchDto.AppendWhereClause(advancedWhereClause);
                return;
            }
            // if until now the query is empty is because only the facility was given
            // so the facility clause is used
            Log.Debug("Building facility where clause");
            var facilitiesClause = BuildAdvancedSearchWhereClause(facilityList, tableName, true);
            searchDto.AppendWhereClause(facilitiesClause);
        }
    }
}
