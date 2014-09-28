using softwrench.sw4.Hapag.Data.Configuration;
using softWrench.sW4.SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.Hapag.Data {

    public class ReportWhereClauseProvider : ISingletonComponent {
        readonly R0017WhereClauseProvider _r0017WhereClauseProvider;

        public ReportWhereClauseProvider(R0017WhereClauseProvider r0017WhereClauseProvider) {
            _r0017WhereClauseProvider = r0017WhereClauseProvider;
        }
        public string HardwareRepairReportWhereClause() {
            var r0017 = _r0017WhereClauseProvider.IncidentWhereClause();
            return "(" + r0017 + ")" + " AND " + HapagQueryConstants.DefaultHardwareRepairReportQuery;
        }
        public string TapeBackupReportWhereClause() {
            var r0017 = _r0017WhereClauseProvider.IncidentWhereClause();
            return "(" + r0017 + ")" + " AND " + HapagQueryConstants.DefaultTapeBackUpReportQuery;
        }
    }
}
