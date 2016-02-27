using System.Collections.Generic;
using cts.commons.persistence;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FirstSolarWorkorderDataSet : BaseWorkorderDataSet {
        private readonly FirstSolarPCSLocationHandler _pcsLocationHandler;

        public FirstSolarWorkorderDataSet(ISWDBHibernateDAO swdbDao, FirstSolarPCSLocationHandler pcsLocationHandler) : base(swdbDao) {
            _pcsLocationHandler = pcsLocationHandler;
        }

        public override string ClientFilter() {
            return "firstsolar";
        }

        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            if (_pcsLocationHandler.IsAdvancedSearch(searchDto)) {
                _pcsLocationHandler.AppendAdvancedSearchWhereClause(application, searchDto, "workorder");
            }
            return base.GetList(application, searchDto);
        }

        public IEnumerable<IAssociationOption> GetFsLocationsOfInterest(OptionFieldProviderParameters parameters) {
            return new List<IAssociationOption>();
        }

        public IEnumerable<IAssociationOption> GetFsSwitchgearLocations(OptionFieldProviderParameters parameters) {
            return new List<IAssociationOption>();
        }
    }
}