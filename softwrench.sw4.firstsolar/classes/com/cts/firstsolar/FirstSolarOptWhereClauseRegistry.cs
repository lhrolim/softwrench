using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
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


            WhereClauseFacade.Register("workorder", "@firstSolarOptWhereClauseRegistry.WorkorderCreationLookup",
                new WhereClauseRegisterCondition {
                    AppContext = new softWrench.sW4.Security.Context.ApplicationLookupContext {
                        MetadataId = "wpcreationlookup"
                    }
                });

            WhereClauseFacade.Register("workorder", SiteClause(false));
            WhereClauseFacade.Register("fsocworkorder", SiteClause(false));

            WhereClauseFacade.Register("workorder", "workorder.status in('INPRG', 'APPR') and workorder.outreq = 1" + SiteClause(true),
                new WhereClauseRegisterCondition {
                    AppContext = new softWrench.sW4.Security.Context.ApplicationLookupContext {
                        Schema = "wplist"
                    }
                });

        }

        public string WorkorderCreationLookup() {
            var ids = Dao.FindByQuery<object>(WorkPackage.NonDeletedWorkorderIds);
            if (ids.Any()) {
                var idsToSearch = BaseQueryUtil.GenerateInString(ids.Select(i => i.ToString()));
                return
                    $"workorder.status in('INPRG', 'APPR') and workorder.outreq = 1 {SiteClause(true)} and workorderid not in ({idsToSearch})";
            }


            return $"workorder.status in('INPRG', 'APPR') and workorder.outreq = 1 {SiteClause(true)} ";
        }


        private static string SiteClause(bool appendAnd) {
            var sb = new StringBuilder();
            if (appendAnd) {
                sb.Append(" and ");
            }
            if (ApplicationConfiguration.IsProd() || ApplicationConfiguration.Profile.StartsWith("uat")) {
                sb.Append(" workorder.siteid in ('1801','1803','1808', '6801') ");
                return sb.ToString();
            }

            sb.Append(" workorder.siteid in ('BEDFORD','1803', 'FS') ");
            return sb.ToString();
        }




    }
}
