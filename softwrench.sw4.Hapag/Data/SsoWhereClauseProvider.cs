using softwrench.sw4.Hapag.Data.Configuration;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using System.Text;

namespace softwrench.sw4.Hapag.Data {
    public class SsoWhereClauseProvider : ISingletonComponent {

        internal const string ServicesWhereClause = @" {0}.commodity in ({1}) ";

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

        enum DashBoardType {
            EuOpenRequests, ActionRequiredRequests, ActionRequiredIncidents
        }

        private static string DoGetWhereClause(string entity, DashBoardType? dashboard = null) {
            var sb = new StringBuilder();

            var ssoListOfServices = GetSsoListOfServices();
            if (ssoListOfServices != null) {
                sb.Append(ServicesWhereClause.Fmt(entity, ssoListOfServices));

            }
            if (dashboard != null && dashboard != DashBoardType.EuOpenRequests) {
                //this first condition goes except for the EuOpenRequests
                if (ssoListOfServices != null) {
                    sb.Append(" and ");
                }
                sb.Append(HapagQueryConstants.ITCActionRequired(entity));
            }

            if (entity == "sr") {
                //we need to do an or with the enduser queries for SR, since the menu is merged
                sb.Append(MergeWithEndUserQuery(dashboard, sb.Length > 0));
            }
            return sb.ToString();
        }

        private static string MergeWithEndUserQuery(DashBoardType? dashboard, bool hasFirstConditionAlready) {
            var pattern = hasFirstConditionAlready ? "or ({0})" : "{0}";
            if (dashboard == null) {
                return pattern.Fmt(HapagQueryConstants.EndUserSR);
            }

            if (DashBoardType.EuOpenRequests.Equals(dashboard)) {
                return pattern.Fmt(HapagQueryConstants.EndUserOpenRequests);
            } if (DashBoardType.ActionRequiredRequests.Equals(dashboard)) {
                return pattern.Fmt(HapagQueryConstants.EndUserActionRequired);
            }
            //should not happen
            return "";
        }

        /// <summary>
        /// retrieves a list of services that should be present in the path specified in the properties.xml, and uses them as commodities filters.
        /// 
        /// The format of the file is as follows:
        /// 
        /// HLC-0777	HLC-SW-SSO-SSO-ANALYZE	HLC-SWG
        /// HLC-0778	HLC-SW-SSO-SSO-CONFIG	HLC-SWG
        /// HLC-0779	HLC-SW-SSO-SSO-HANDLING	HLC-SWG
        /// HLC-0780	HLC-SW-SSO-SSO-OTHER	HLC-SWG
        /// HLC-0781	HLC-SW-SSO-SSO-OUTAGE	HLC-SWG
        /// 
        /// Just the first "column" is used
        /// </summary>
        /// <returns></returns>
        private static string GetSsoListOfServices() {
            string services = null;
            var path = ApplicationConfiguration.SertiveItSsoServicesQueryPath;
            if (string.IsNullOrEmpty(path)) {
                return null;
            }
            if (!System.IO.File.Exists(@path)) {
                return null;
            }
            foreach (var row in System.IO.File.ReadAllLines(@path)) {
                var serviceAux = row.Split(null);
                if (serviceAux.Length >= 1) {
                    services += "'" + serviceAux[0] + "',";
                }
            }
            if (!string.IsNullOrEmpty(services)) {
                services = services.ReplaceLastOccurrence(",", "");
            }
            return services;
        }
    }
}
