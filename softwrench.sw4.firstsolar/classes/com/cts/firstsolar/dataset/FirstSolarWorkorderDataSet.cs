using System.Collections.Generic;
using cts.commons.persistence;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FirstSolarWorkorderDataSet : BaseWorkorderDataSet {
        private readonly FirstSolarAdvancedSearchHandler _advancedSearchHandler;

        public FirstSolarWorkorderDataSet(ISWDBHibernateDAO swdbDao, FirstSolarAdvancedSearchHandler advancedSearchHandler) : base(swdbDao) {
            _advancedSearchHandler = advancedSearchHandler;
        }

        public override string ClientFilter() {
            return "firstsolar";
        }

        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            if (_advancedSearchHandler.IsAdvancedSearch(searchDto)) {
                _advancedSearchHandler.AppendAdvancedSearchWhereClause(application, searchDto, "workorder");
            }
            return base.GetList(application, searchDto);
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
    }
}