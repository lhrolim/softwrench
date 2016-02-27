﻿using System.Collections.Generic;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    class FirstSolarLocationDataSet : BaseLocationDataSet {

        public override string ClientFilter() {
            return "firstsolar";
        }

        private readonly FirstSolarPCSLocationHandler _pcsLocationHandler;

        public FirstSolarLocationDataSet(FirstSolarPCSLocationHandler pcsLocationHandler) {
            _pcsLocationHandler = pcsLocationHandler;
        }


        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var quickSearchData = searchDto.QuickSearchData;

            if (string.IsNullOrEmpty(quickSearchData)) {
                if (_pcsLocationHandler.IsAdvancedSearch(searchDto)) {
                    _pcsLocationHandler.AppendAdvancedSearchWhereClause(application, searchDto, "location");
                }
                return base.GetList(application, searchDto);
            }

            var query = _pcsLocationHandler.DoGetPCSQuery(quickSearchData,"location");
            if (query != null) {
                searchDto.AppendWhereClause(query);
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
