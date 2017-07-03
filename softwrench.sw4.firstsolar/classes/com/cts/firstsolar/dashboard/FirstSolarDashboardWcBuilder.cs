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

        protected DateTime Last5Days = DateTime.Now;
        protected List<string> WosWithPackages = new List<string>();

        protected DateTime Last30Days = DateTime.Now;
        protected DateTime Last31Days = DateTime.Now;
        protected DateTime Last60Days = DateTime.Now;

        [Import]
        public ISWDBHibernateDAO SWDBDao { get; set; }

        public virtual string BuildWhereClause(string entityName, SearchRequestDto searchDto = null) {
            if (!"workorder".Equals(entityName) || searchDto?.Key == null) {
                return "";
            }

            if (FirstSolarDashboardInitializer.IncomingPanelSchemaId.Equals(searchDto.Key.SchemaId)) {
                return MaintenanceIncomingQuery();
            }

            if (FirstSolarDashboardInitializer.BuildPanelSchemaId.Equals(searchDto.Key.SchemaId)) {
                return MaintenanceBuildQuery();
            }

            if (FirstSolarDashboardInitializer.BuildPanel290SchemaId.Equals(searchDto.Key.SchemaId)) {
                return MaintenanceBuild290Query();
            }

            return "1=1";
        }

        public virtual IDictionary<string, object> GetParameters() {
            return new Dictionary<string, object>() {
                { "last5Days", Last5Days },
                { "wosWithPackage", WosWithPackages },
                { "last30Days", Last30Days },
                { "last60Days", Last60Days }
            };
        }

        public virtual string MaintenanceDashQuery() {
            var siteClause = (ApplicationConfiguration.IsProd() || ApplicationConfiguration.Profile.StartsWith("uat")) ? " and workorder.siteid = '1801' " : "";
            return "workorder.status in ('APPR','INPRG') and workorder.worktype = 'PM' and workorder.outreq = 1" + siteClause;
        }

        protected virtual string MaintenanceIncomingQuery() {
            Last5Days = DateUtil.BeginOfDay(DateUtil.ParsePastAndFuture("5days", -1));
            WosWithPackages = SWDBDao.FindByNativeQuery("select workorderid from opt_workpackage where createddate > :p0", Last5Days).Select(dict => dict["workorderid"]).ToList();
            if (WosWithPackages.Any()) {
                return "workorder.reportdate >= :last5Days and workorder.workorderid not in (:wosWithPackage)";
            }
            return "workorder.reportdate >= :last5Days";
        }

        protected virtual string MaintenanceBuildQuery() {
            Last30Days = DateUtil.BeginOfDay(DateUtil.ParsePastAndFuture("30days", -1));
            Last5Days = DateUtil.BeginOfDay(DateUtil.ParsePastAndFuture("5days", -1));
            return "workorder.reportdate < :last5Days and workorder.reportdate >= :last30Days";
        }

        protected virtual string MaintenanceBuild290Query() {
            Last60Days = DateUtil.BeginOfDay(DateUtil.ParsePastAndFuture("60days", -1));
            Last30Days = DateUtil.BeginOfDay(DateUtil.ParsePastAndFuture("30days", -1));
            return "workorder.reportdate < :last30Days and workorder.reportdate >= :last60Days";
        }
    }
}
