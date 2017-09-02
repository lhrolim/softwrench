using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dashboard {
    public class FirstSolarDashboardWcBuilder : IDynamicWhereBuilder {

        protected DateTime Last30Days = DateTime.Now;
        protected List<string> WosWithPackages = new List<string>();

        [Import]
        public ISWDBHibernateDAO SWDBDao { get; set; }

        public virtual string BuildWhereClause(string entityName, SearchRequestDto searchDto = null) {
            if (!"workorder".Equals(entityName) || searchDto?.Key == null) {
                return "";
            }

            if (FirstSolarDashboardInitializer.IncomingPanelSchemaId.Equals(searchDto.Key.SchemaId)) {
                return MaintenanceIncomingQuery();
            }

            if (FirstSolarDashboardInitializer.IncomingCmPanelSchemaId.Equals(searchDto.Key.SchemaId)) {
                return MaintenanceIncomingQuery();
            }

            //            if (FirstSolarDashboardInitializer.BuildPanelSchemaId.Equals(searchDto.Key.SchemaId)) {
            //                return MaintenanceBuildQuery();
            //            }
            //
            //            if (FirstSolarDashboardInitializer.PmBuildPanelSchemaId.Equals(searchDto.Key.SchemaId)) {
            //                return MaintenanceBuild290Query();
            //            }

            return "1=1";
        }

        public virtual IDictionary<string, object> GetParameters() {
            return new Dictionary<string, object>() {
                { "wosWithPackage", new List<string>(WosWithPackages) },
                { "last30Days", Last30Days }
            };
        }

        public virtual string MaintenanceDashQuery() {
            return "workorder.status in ('APPR','INPRG') and workorder.worktype = 'PM' and workorder.outreq = 1" + SiteClause();
        }

        private static string SiteClause() {
            return (ApplicationConfiguration.IsProd() || ApplicationConfiguration.Profile.StartsWith("uat")) ? " and workorder.siteid in ('1801','1803','1808','6801' ) " : "";
        }

        public virtual string MaintenanceDashMixedQuery() {
            return "workorder.status in ('APPR','INPRG') and workorder.worktype in ('PM','WO') and workorder.outreq = 1" + SiteClause();
        }

        public virtual string MaintenanceDashBuild30PQuery() {
            return @"workorder.status in ('APPR','INPRG') {0} and workorder.outreq = 1
                    and
                    workorder.worktype = 'PM'  and workorder.reportdate >= @past(30days )
                    ".Fmt(SiteClause());
        }

        public virtual string MaintenanceDashBuild290PMQuery() {
            return @"workorder.status in ('APPR','INPRG') {0} and workorder.outreq = 1
                    and
                    (workorder.worktype = 'PM'  and workorder.reportdate < @past(30days))".Fmt(SiteClause());
        }

        public virtual string MaintenanceDashBuild290CMQuery() {
            return @"workorder.status in ('APPR','INPRG') {0} and workorder.outreq = 1
                    and        
                    (workorder.worktype = 'WO')".Fmt(SiteClause());
        }

        public virtual string MaintenanceCmDashQuery() {
            return "workorder.status in ('APPR','INPRG') and workorder.worktype != 'pm' and workorder.outreq = 1" + SiteClause();
        }

        protected virtual string MaintenanceIncomingQuery() {
            // TODO: add a flag on wp to know if its completed to not search all wps on this query
            WosWithPackages = SWDBDao.FindByNativeQuery("select workorderid from opt_workpackage where deleted = 'false'").Select(dict => dict["workorderid"]).ToList();
            return WosWithPackages.Any() ? "workorder.workorderid not in (:wosWithPackage)" : "1=1";
        }

        protected virtual string MaintenanceIncomingCmQuery() {
            // TODO: add a flag on wp to know if its completed to not search all wps on this query
            WosWithPackages = SWDBDao.FindByNativeQuery("select workorderid from opt_workpackage where deleted = 'false'").Select(dict => dict["workorderid"]).ToList();
            return WosWithPackages.Any() ? "workorder.workorderid not in (:wosWithPackage)" : "1=1";
        }

        protected virtual string MaintenanceBuildQuery() {
            Last30Days = DateUtil.BeginOfDay(DateUtil.ParsePastAndFuture("30days", -1));
            return "workorder.reportdate >= :last30Days";
        }

        protected virtual string MaintenanceBuild290Query() {
            Last30Days = DateUtil.BeginOfDay(DateUtil.ParsePastAndFuture("30days", -1));
            return "workorder.reportdate < :last30Days";
        }
    }
}
