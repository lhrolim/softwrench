using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using NHibernate.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dashboard;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FirstSolarWorkorderDataSet : BaseWorkorderDataSet {
        private readonly FirstSolarAdvancedSearchHandler _advancedSearchHandler;

        [Import]
        public EntityRepository EntityRepository { get; set; }

        public FirstSolarWorkorderDataSet(ISWDBHibernateDAO swdbDao, FirstSolarAdvancedSearchHandler advancedSearchHandler) : base(swdbDao) {
            _advancedSearchHandler = advancedSearchHandler;
        }

        public override string ClientFilter() {
            return "firstsolar";
        }

        public override string ApplicationName() {
            return "workorder,otherworkorder";
        }

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            if (_advancedSearchHandler.IsAdvancedSearch(searchDto)) {
                _advancedSearchHandler.AppendAdvancedSearchWhereClause(application, searchDto, "workorder");
            }
            if (IsMaintenanceBuildDash() || IsWorkPackageGrid()) {
                return await GetMaintenanceBuildDashList(application, searchDto);
            }
            return await base.GetList(application, searchDto);
        }

    

        public override SearchRequestDto FilterAssets(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var location = (string)parameters.OriginalEntity.GetAttribute("location");
            if (location == null) {
                Log.Debug("Done: No locations => no filter for location on asset filter.");
                return filter;
            }

            var clause = new StringBuilder("(");
            clause.Append("asset.location = '").Append(location).Append("'");
            clause.Append(" OR asset.location in ( ");
            clause.Append("select a.location from locancestor a ");
            clause.Append("where a.ancestor = '").Append(location).Append("')");
            clause.Append(")");
            Log.Debug(string.Format("Done where clause to filter assets from location: {0}", clause));
            filter.AppendWhereClause(clause.ToString());

            return filter;
        }

        /// <summary>
        /// Kind of a dummy. The real list is got from FirstSolarAdvancedSearchController when a facility is selected.
        /// </summary>
        public IEnumerable<IAssociationOption> GetFsLocationsOfInterest(OptionFieldProviderParameters parameters) {
            return new List<IAssociationOption>();
        }

        /// <summary>
        /// Kind of a dummy. The real list is got from FirstSolarAdvancedSearchController when a facility is selected.
        /// </summary>
        public IEnumerable<IAssociationOption> GetFsSwitchgearLocations(OptionFieldProviderParameters parameters) {
            return new List<IAssociationOption>();
        }

        /// <summary>
        /// Kind of a dummy. The real list is got from FirstSolarAdvancedSearchController when a facility is selected.
        /// </summary>
        public IEnumerable<IAssociationOption> GetFsPcsLocations(OptionFieldProviderParameters parameters) {
            return new List<IAssociationOption>();
        }

        public SearchRequestDto ZeroAttachmentsWhereClause(CompositionPreFilterFunctionParameters parameter) {
            //enforcing no attachments are brought for group workorders
            parameter.BASEDto.AppendWhereClause("1=0");
            return parameter.BASEDto;
        }

        #region maintenance dash
        private bool IsMaintenanceBuildDash() {
            var context = ContextLookuper.LookupContext();
            return context?.ApplicationLookupContext?.Schema != null && context.ApplicationLookupContext.Schema.EqualsAny(FirstSolarDashboardInitializer.PmBuildPanelSchemaId, FirstSolarDashboardInitializer.CmBuildPanelSchemaId);
        }

        private bool IsWorkPackageGrid()
        {
            var context = ContextLookuper.LookupContext();
            return context?.ApplicationLookupContext?.Schema != null && context.ApplicationLookupContext.Schema.EqualsAny("wplist");
        }

        private async Task<ApplicationListResult> InnerGetMaintenanceBuildDashList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var replaced = false;
            var oldSearchSort = searchDto.SearchSort;
            if (searchDto.SearchSort != null) {
                replaced = true;
                searchDto.SearchSort = searchDto.SearchSort.Replace("#wpnum", "wonum");
            }
            var result = await base.GetList(application, searchDto);
            if (replaced) {
                result.PageResultDto.SearchSort = oldSearchSort;
            }
            return result;
        }

        private async Task<ApplicationListResult> GetMaintenanceBuildDashList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var workPackageFilters = GetWorkPackageFilters(searchDto);
            SanitizeDTOForMaximo(searchDto);
            var wpData = await LookupWorkPackageData(workPackageFilters);
            var withRestrictionsSearchDto = HandleWorkPackageRestrictions(searchDto, wpData);
            var result = await InnerGetMaintenanceBuildDashList(application, withRestrictionsSearchDto);
            return BuildCombinedProjectedData(result, wpData);
        }

        private async Task<IDictionary<string, DataMap>> LookupWorkPackageData(Dictionary<string, SearchParameter> wpFilters) {
            var wpSearchDTO = new PaginatedSearchRequestDto();
            wpFilters.ForEach(pair => {
                wpSearchDTO.AppendSearchEntry(pair.Key, pair.Value.RawValue);
            });
            //            wpSearchDTO.WhereClause = DefaultValuesBuilder.ConvertAllValues(" WorkPackage_.createddate > :createddate ", SecurityFacade.CurrentUser());
            wpSearchDTO.PageSize = 1000;
            var now = DateTime.Now;
            now = DateUtil.BeginOfDay(now.AddDays(-60));

            wpSearchDTO.AppendSearchEntry(":createddate", ">=" + now.ToString("yyyy/MM/dd"));

            return await EntityRepository.GetGrouppingById(GetWorkPackageEntity(), wpSearchDTO);
        }

        private ApplicationListResult BuildCombinedProjectedData(ApplicationListResult baseList, IDictionary<string, DataMap> wpData) {
            var context = ContextLookuper.LookupContext();
            var schema = context.ApplicationLookupContext.Schema;

            foreach (var item in baseList.ResultObject) {
                //TODO: fix call hierarchy, as it´s always a DataMap
                var dm = item as DataMap;
                if (dm == null) {
                    //not happening
                    continue;
                }
                var workOrderId = dm.GetStringAttribute("workorderid");
                var wp = wpData.ToList().Select(pair => pair.Value).FirstOrDefault(wpDm => workOrderId.Equals(wpDm["workorderid"].ToString()));
                if (wp == null) {
                    continue;
                }
                foreach (var field in wp.Fields) {
                    if (!dm.ContainsKey("#" + field.Key)) {
                        dm.Add("#" + field.Key, field.Value);
                    }
                }

                if (dm.ContainsKey("#buildcomplete") && (dm["#buildcomplete"] is bool) && (dm["#buildcomplete"] as bool?).Value) {
                    dm["#colorcode"] = "#39b54a"; // green
                    continue;
                }
                if (FirstSolarDashboardInitializer.PmBuildPanelSchemaId.Equals(schema)) {
                    dm["#colorcode"] = "#f65752"; // red
                    continue;
                }

                if (!dm.ContainsKey("reportdate") || !(dm["reportdate"] is DateTime)) {
                    continue;
                }

                var reportDate = (dm["reportdate"] as DateTime?).Value;
                var today = DateUtil.BeginOfDay(DateTime.Now);
                var reportDay = DateUtil.BeginOfDay(reportDate);
                var diffInDays = (today - reportDay).Days;
                dm["#colorcode"] = diffInDays <= 25 ? "#39b54a" : "#f2d935"; // green or yellow
            }
            return baseList;
        }

        private static Dictionary<string, SearchParameter> GetWorkPackageFilters(SearchRequestDto searchDto) {
            var wpSearchParams = new Dictionary<string, SearchParameter>();
            var searchParameters = searchDto.GetParameters();
            if (searchParameters == null) {
                return wpSearchParams;
            }
            searchParameters.ForEach(pair => {
                if (pair.Key.StartsWith("#")) {
                    wpSearchParams.Add(pair.Key.Substring(1), pair.Value);
                }
            });
            return wpSearchParams;
        }

        private static PaginatedSearchRequestDto HandleWorkPackageRestrictions(PaginatedSearchRequestDto searchDto, IDictionary<string, DataMap> packageDatamap) {
            var whereClause = packageDatamap.Count == 0 ? "1=0" : $" workorderid in ({BaseQueryUtil.GenerateInString(packageDatamap.Values, "workorderid")})";
            searchDto.AppendWhereClause(whereClause);
            return searchDto;
        }

        private static SlicedEntityMetadata GetWorkPackageEntity() {
            var key = new ApplicationMetadataSchemaKey("dashlist");
            var user = SecurityFacade.CurrentUser();
            var applicationMetadata = MetadataProvider.Application("_WorkPackage").ApplyPolicies(key, user, ClientPlatform.Web);
            return MetadataProvider.SlicedEntityMetadata(applicationMetadata);
        }

        private static void SanitizeDTOForMaximo(SearchRequestDto searchDto) {
            if (searchDto.ValuesDictionary == null) {
                return;
            }
            var transientKeys = new List<string>(searchDto.ValuesDictionary.Keys.Where(k => k.StartsWith("#")));
            foreach (var key in transientKeys) {
                searchDto.ValuesDictionary.Remove(key);
            }
        }
        #endregion
    }
}
