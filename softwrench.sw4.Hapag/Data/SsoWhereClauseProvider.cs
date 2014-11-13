using softwrench.sw4.Hapag.Data.Configuration;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.Security.Context;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using System.Text;

namespace softwrench.sw4.Hapag.Data {
    public class SsoWhereClauseProvider : ISingletonComponent {

        private readonly IContextLookuper _contextLookuper;

        internal const string ServicesWhereClause = @" {0}.commodity in ({1}) ";

        public SsoWhereClauseProvider(IContextLookuper contextLookuper) {
            _contextLookuper = contextLookuper;
        }

        public string DashboardServiceRequestWhereClause() {
            return DoGetWhereClause("sr", true);
        }

        public string DashboardIncidentWhereClause() {
            return DoGetWhereClause("incident", true);
        }

        public string ServiceRequestWhereClause() {
            return DoGetWhereClause("sr", false);
        }

        public string IncidentWhereClause() {
            return DoGetWhereClause("incident", false);
        }

        public string ProblemWhereClause() {
            return DoGetWhereClause("problem", false);
        }

        private string DoGetWhereClause(string entity, bool dashboard) {
            var sb = new StringBuilder();

            var ssoListOfServices = GetSsoListOfServices();
            if (ssoListOfServices != null) {
                sb.Append(ServicesWhereClause.Fmt(entity, ssoListOfServices));
                if (dashboard) {
                    sb.Append(" and ");
                }
            }
            if (dashboard) {
                sb.Append(HapagQueryConstants.ITCActionRequired(entity));
            }
            if (entity == "sr") {
                if (!dashboard) {
                    if (ssoListOfServices != null) {
                        sb.Append(" or ( {0} ) ".Fmt(HapagQueryConstants.EndUserSR));
                    } else {
                        sb.Append(HapagQueryConstants.EndUserSR);
                    }
                } else {
                    sb.Append(" or ( {0} ) ".Fmt(HapagQueryConstants.EndUserActionRequired));
                }
            }
            return sb.ToString();
        }


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
