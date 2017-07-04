using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar {
    public class FirstSolarOptWhereClauseRegistry : ISWEventListener<ApplicationStartedEvent> {

        [Import]
        public IWhereClauseFacade WhereClauseFacade { get; set; }


        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {

            var engineerWhereClause = ApplicationConfiguration.IsProd() || ApplicationConfiguration.IsUat()
                ? "displayname in ('Tom Studer','Mark Atchley','Stephen Swan','Frank Shiflett','Mathew Ciochetto','Reece Lowry','RJ Hardin','Robert Bradford')"
                : "1=1";

            WhereClauseFacade.Register("person", engineerWhereClause,
                new WhereClauseRegisterCondition {
                    AppContext = new softWrench.sW4.Security.Context.ApplicationLookupContext {
                        MetadataId = "maintenanceengineerlookup"
                    }
                });

            
            var siteClause = (ApplicationConfiguration.IsProd() || ApplicationConfiguration.Profile.StartsWith("uat")) ? " and workorder.siteid = '1801' " : "";
            WhereClauseFacade.Register("workorder", "workorder.status in('INPRG', 'APPR', 'WOEN', 'ENRV', 'HOLD', 'WAPPR')" + siteClause,
                new WhereClauseRegisterCondition {
                    AppContext = new softWrench.sW4.Security.Context.ApplicationLookupContext {
                        MetadataId = "wpcreationlookup"
                    }
                });
        }
    }
}
