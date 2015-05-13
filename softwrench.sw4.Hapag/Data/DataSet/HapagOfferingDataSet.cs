using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.Util;
using System;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagOfferingDataSet : HapagBaseApplicationDataSet {


        private const string Explanation = 
@"- TSM Backup / Restore (in Abhängigkeit von Produktions-/Testsystem)
- Basis-Monitoring (TEC, SRM)
- IBM Management SW (z.B. TSCM, TAD4D, …)
- Security Settings (GSD331)
- Aufnahme des neuen Servers als Firewall-Objekt mit den Basis-Standardregeln und Aufnahme in bestehende Gruppe
- Einbindung in QRadar (SIEM) nach den Standardvorgaben";

        protected override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result = base.GetList(application, searchDto);
            return result;
        }

        protected override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application,
            InMemoryUser user, DetailRequest request) {
            var baseDetail = base.GetApplicationDetail(application, user, request);
            var resultObject = baseDetail.ResultObject;
            resultObject.SetAttribute("minstartdate", DateUtil.BeginOfDay(DateTime.Now.AddBusinessDays(3)));
            resultObject.SetAttribute("explanation", Explanation);
            return baseDetail;
        }


        public override string ApplicationName() {
            return "offering";
        }


    }
}
