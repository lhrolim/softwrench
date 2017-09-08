﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar {
    public class FirstSolarOptWhereClauseRegistry : ISWEventListener<ApplicationStartedEvent> {

        [Import]
        public IWhereClauseFacade WhereClauseFacade { get; set; }

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }


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


            var siteClause = SiteClause();
            WhereClauseFacade.Register("workorder", "workorder.status in('INPRG', 'APPR') and workorder.outreq = 1" + siteClause,
                new WhereClauseRegisterCondition {
                    AppContext = new softWrench.sW4.Security.Context.ApplicationLookupContext {
                        MetadataId = "wpcreationlookup"
                    }
                });

            WhereClauseFacade.Register("workorder", "workorder.status in('INPRG', 'APPR') and workorder.outreq = 1" + siteClause,
                new WhereClauseRegisterCondition {
                    AppContext = new softWrench.sW4.Security.Context.ApplicationLookupContext {
                        Schema = "wplist"
                    }
                });

        }


        private static string SiteClause() {
            return (ApplicationConfiguration.IsProd() || ApplicationConfiguration.Profile.StartsWith("uat")) ? " and workorder.siteid in ('1801','1803','1808', '6801') " : "";
        }




    }
}
