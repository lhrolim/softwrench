using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.SimpleInjector;

namespace softwrench.sw4.Hapag.Data.DataSet {
    public class HapagCIDataSet : BaseApplicationDataSet {


        private SWDBHibernateDAO _dao;

        private SWDBHibernateDAO GetDAO() {
            if (_dao == null) {
                _dao = SimpleInjectorGenericFactory.Instance.GetObject<SWDBHibernateDAO>(typeof(SWDBHibernateDAO));
            }
            return _dao;
        }


        public SearchRequestDto AppendCiTicketHistoryQuery(CompositionPreFilterFunctionParameters preFilter) {
            var dto = preFilter.BASEDto;
            dto.SearchValues = null;
            dto.SearchParams = null;
            var assetNum = preFilter.OriginalEntity.GetAttribute("assetnum");
            dto.AppendWhereClauseFormat("ticketid in (select recordkey from MULTIASSETLOCCI multi where multi.assetnum = '{0}' and RECORDCLASS in ({1}) )", assetNum, "'CHANGE','INCIDENT','PROBLEM','SR'");

            return dto;
        }

        public SearchRequestDto AppendImacMultiLocciTicketHistoryQuery(CompositionPreFilterFunctionParameters preFilter) {
            var dto = preFilter.BASEDto;
            dto.SearchValues = null;
            dto.SearchParams = null;
            var assetNum = preFilter.OriginalEntity.GetAttribute("assetnum");
            //as of HAP-882
            dto.AppendWhereClauseFormat(@"ticketid in (select recordkey from MULTIASSETLOCCI multi where multi.assetnum = '{0}' and RECORDCLASS in ({1}) ) " +
                                        "or (imac.classificationid = '81515700' and imac.description = 'Decommission of {0}')", assetNum, "'CHANGE','INCIDENT','PROBLEM','SR'");
            dto.IgnoreWhereClause = true;
            return dto;
        }

        protected override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var baseDetail = base.GetApplicationDetail(application, user, request);
            FetchRemarks(baseDetail.ResultObject);
            return baseDetail;
        }

        private void FetchRemarks(DataMap resultObject) {
            var ciId = resultObject.GetAttribute("ciid");
            var extraAttributte = GetDAO().FindSingleByQuery<ExtraAttributes>(ExtraAttributes.ByMaximoTABLEIdAndAttribute, "ci", ciId.ToString(), "remarks");
            if (extraAttributte != null) {
                resultObject.SetAttribute(extraAttributte.AttributeName, extraAttributte.AttributeValue);
            }
        }

        public override string ApplicationName() {
            return "ci";
        }

        public override string ClientFilter() {
            return "hapag";
        }


    }
}
