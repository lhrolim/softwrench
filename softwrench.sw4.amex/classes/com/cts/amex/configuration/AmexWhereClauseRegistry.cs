using cts.commons.simpleinjector;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.amex.classes.com.cts.amex.configuration {
    public class AmexWhereClauseRegistry : ISingletonComponent {
        public AmexWhereClauseRegistry(IWhereClauseFacade facade) {
            if (ApplicationConfiguration.ClientName != "amex") {
                return;
            }
            facade.Register("asset", "@amexWhereClauseRegistry.RrfdListWhereClauseProvider", new WhereClauseRegisterCondition() {
                AppContext = new ApplicationLookupContext(){
                Schema = "rrfdlist"
            }
            });
        }

        public string RrfdListWhereClauseProvider() {
            var user = SecurityFacade.CurrentUser();
            //amex relies on DCI_SITE rather than siteid for filtering their data
            var dciSite = user.Genericproperties["DCI_SITE"];
            return string.Format("ASSET.DCI_SITE = '{0}' AND ASSET.STATUS in ('NOT READY','SPACE PLANNED')" , dciSite);
        }

    }
}
