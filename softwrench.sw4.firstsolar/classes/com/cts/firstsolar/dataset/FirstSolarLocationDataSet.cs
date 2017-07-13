using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FirstSolarLocationDataSet : BaseLocationDataSet {

        public override string ClientFilter() {
            return "firstsolar";
        }

        private readonly FirstSolarPCSLocationHandler _pcsLocationHandler;
        private readonly FirstSolarAdvancedSearchHandler _advancedSearchHandler;

        public FirstSolarLocationDataSet(FirstSolarPCSLocationHandler pcsLocationHandler, FirstSolarAdvancedSearchHandler advancedSearchHandler) {
            _pcsLocationHandler = pcsLocationHandler;
            _advancedSearchHandler = advancedSearchHandler;
        }


        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var quickSearchDTO = searchDto.QuickSearchDTO;

            if (quickSearchDTO == null) {
                if (_advancedSearchHandler.IsAdvancedSearch(searchDto)) {
                    _advancedSearchHandler.AppendAdvancedSearchWhereClause(application, searchDto, "location");
                }
                return await base.GetList(application, searchDto);
            }

            var query = _pcsLocationHandler.DoGetPCSQuery(quickSearchDTO.QuickSearchData, "location");
            if (query != null) {
                searchDto.AppendWhereClause(query);
            }

            return await base.GetList(application, searchDto);
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

        public string FacilityQuery(string context) {
            if (!ApplicationConfiguration.IsProd()) {
                return "SUBSTRING({0}.location, 0, 5)".Fmt(context);
            }
            return "CASE WHEN exists (select * from onmparms o where {0}.location like o.value + '%') THEN (select G.scadA_GUID from onmparms o left join GLOBALFEDPRODUCTION.GlobalFed.Business.vwsites G on  (o.description=G.assettitle or o.value=G.maximo_LocationID) where o.parameter='PlantID' and {0}.location like o.value + '%') WHEN 1=1 then SUBSTRING({0}.location, 0, 5) END".Fmt(context);
        }
    }
}
