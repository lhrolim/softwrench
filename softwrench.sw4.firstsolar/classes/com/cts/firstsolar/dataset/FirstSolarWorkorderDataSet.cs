﻿using System.Collections.Generic;
using System.Text;
using cts.commons.persistence;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FirstSolarWorkorderDataSet : BaseWorkorderDataSet {
        private readonly FirstSolarAdvancedSearchHandler _advancedSearchHandler;

        public FirstSolarWorkorderDataSet(ISWDBHibernateDAO swdbDao, FirstSolarAdvancedSearchHandler advancedSearchHandler) : base(swdbDao) {
            _advancedSearchHandler = advancedSearchHandler;
        }

        public override string ClientFilter() {
            return "firstsolar";
        }

        public override string ApplicationName() {
            return "workorder,otherworkorder";
        }

        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            if (_advancedSearchHandler.IsAdvancedSearch(searchDto)) {
                _advancedSearchHandler.AppendAdvancedSearchWhereClause(application, searchDto, "workorder");
            }
            return base.GetList(application, searchDto);
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

    }
}
