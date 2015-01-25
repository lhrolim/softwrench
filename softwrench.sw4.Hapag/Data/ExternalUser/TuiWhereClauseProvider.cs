using System.Text;
using softwrench.sw4.Hapag.Data.Configuration;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data.ExternalUser {
    public class TuiWhereClauseProvider : ExternalUserWhereClauseProvider {
        public string DashboardServiceRequestWhereClause() {
            return DoGetWhereClause("sr", DashBoardType.ActionRequiredRequests);
        }

        public string EuOpenRequests() {
            return DoGetWhereClause("sr", DashBoardType.EuOpenRequests);
        }

        public string DashboardIncidentWhereClause() {
            return DoGetWhereClause("incident", DashBoardType.ActionRequiredIncidents);
        }

        public string ServiceRequestWhereClause() {
            return DoGetWhereClause("sr");
        }

        public string IncidentWhereClause() {
            return DoGetWhereClause("incident");
        }
        public string ProblemWhereClause() {
            return DoGetWhereClause("problem");
        }


        protected override string GetExternalFilePath() {
            return ApplicationConfiguration.SertiveItTuiServicesQueryPath;
        }
    }
}
