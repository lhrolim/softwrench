using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using System;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagOfferingDataSet : HapagBaseApplicationDataSet {

        private I18NResolver _resolver;

        public I18NResolver Resolver {
            get {
                if (_resolver != null) {
                    return _resolver;
                }
                _resolver =
                    SimpleInjectorGenericFactory.Instance.GetObject<I18NResolver>(typeof(I18NResolver));
                return _resolver;
            }
        }


        private const string Explanation =
@"- TSM Backup / Restore (depending on production/test system)
- basic monitoring (TEC, SRM)
- IBM Management SW (e.g. TSCM, TAD4D, …)
- security settings (GSD331)
- the new server is added as firewall object with basic default rules and is included in existing group
- integration in QRadar (SIEM) according to default specification";

        protected override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result = base.GetList(application, searchDto);
            return result;
        }

        protected override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application,
            InMemoryUser user, DetailRequest request) {
            var baseDetail = base.GetApplicationDetail(application, user, request);
            var resultObject = baseDetail.ResultObject;
            resultObject.SetAttribute("minstartdate", DateUtil.BeginOfDay(DateTime.Now.AddBusinessDays(3)));
            resultObject.SetAttribute("explanation", Resolver.I18NValue("offering.explanation", Explanation));
            return baseDetail;
        }


        public override string ApplicationName() {
            return "offering";
        }


    }
}
