﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration {
    public class FirstSolarWhereClauseRegistry : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {


        private readonly IWhereClauseFacade _whereClauseFacade;

        /// <summary>
        /// Brings all assignments, where exists a workorder of interest, narrowing by the facility query that will be replaced at {0}
        /// </summary>
        private const string AssignedWhereClause =
            @"exists (select 1 from workorder as workorder_ where workorder_.wonum = assignment.wonum and workorder_.siteid = assignment.siteid and workorder_.orgid = assignment.orgid and 
                workorder_.status not in ('comp','can','close') and workorder_.status in ('APPR','INPRG','WAPPR') and {0} )";


        private const string WOAssignedWhereClause =
        @"workorder.status not in ('comp','can','close') and workorder.status in ('APPR','INPRG','WAPPR') and workorder.siteid = @siteid 
            and 
            exists (select 1 from assignment a where workorder.wonum = a.wonum and workorder.siteid = a.siteid and workorder.orgid = a.orgid 
                and a.laborcode = '@user.properties['laborcode']' and a.scheduledate >= @past(1week) and a.scheduledate <= @future(1week))";

        /// <summary>
        /// Brings all workorders excluding the ones which already have an assignment for the current user on the week. Facility filters will be applied on top of this to narrow the list
        /// </summary>
        private const string WOGroupByBaseWhereClause =
          @"workorder.status not in ('comp','can','close') and workorder.status in ('APPR','INPRG','WAPPR') and workorder.siteid = @siteid
            and not
            exists (select 1 from assignment a where workorder.wonum = a.wonum and workorder.siteid = a.siteid and workorder.orgid = a.orgid 
                and a.laborcode = '@user.properties['laborcode']' and a.scheduledate >= @past(1week) and a.scheduledate <= @future(1week))";


        private const string UserLaborWhereClause = "labor.laborcode='@user.properties['laborcode']' and labor.orgid=@orgid";
        private const string UserLaborCraftWhereClause = "laborcraftrate.laborcode='@user.properties['laborcode']' and laborcraftrate.orgid=@orgid";


        public FirstSolarWhereClauseRegistry(IWhereClauseFacade whereClauseFacade) {
            _whereClauseFacade = whereClauseFacade;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (!ApplicationConfiguration.IsClient("firstsolar")) {
                //to avoid issues on dev environments
                return;
            }

            var offLineCondition = new WhereClauseRegisterCondition() { Alias = "offline", OfflineOnly = true, Global = true };

            _whereClauseFacade.Register("workorder", WOAssignedWhereClause, offLineCondition);
            _whereClauseFacade.Register("otherworkorder", "@firstSolarWhereClauseRegistry.WorkordersByGroup", offLineCondition);


            _whereClauseFacade.Register("assignment", "@firstSolarWhereClauseRegistry.AssignmentsByGroup", offLineCondition);


            _whereClauseFacade.Register("offlinelocation", "@firstSolarWhereClauseRegistry.LocationWhereClauseByFacility", offLineCondition);
            _whereClauseFacade.Register("offlineasset", "@firstSolarWhereClauseRegistry.AssetWhereClauseByFacility", offLineCondition);
            _whereClauseFacade.Register("locancestor", "@firstSolarWhereClauseRegistry.LocAncestorWhereClauseByFacility", offLineCondition);
            _whereClauseFacade.Register("synstatus", "value in ('WOEN','COMP')", offLineCondition);
            _whereClauseFacade.Register("labor", UserLaborWhereClause, offLineCondition);
            _whereClauseFacade.Register("laborcraftrate", UserLaborCraftWhereClause, offLineCondition);
        }

        public string AssignmentsByGroup() {
            var user = SecurityFacade.CurrentUser();
            var locationQuery = "1!=1";
            if (user.Genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                var facilities = (IEnumerable<string>)user.Genericproperties[FirstSolarConstants.FacilitiesProp];
                var enumerable = facilities as IList<string> ?? facilities.ToList();
                if (facilities != null && enumerable.Any()) {
                    locationQuery = BaseQueryUtil.GenerateOrLikeString("workorder_.location", enumerable.Select(f => f + "%"));
                }
            }
            var baseQuery = AssignedWhereClause.Fmt(locationQuery);
            return DefaultValuesBuilder.ConvertAllValues(baseQuery, user);
        }

        public string WorkordersByGroup() {
            var user = SecurityFacade.CurrentUser();
            var sb = new StringBuilder();
            sb.Append(DefaultValuesBuilder.ConvertAllValues(WOGroupByBaseWhereClause, user));
            if (user.Genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                var facilities = (IEnumerable<string>)user.Genericproperties[FirstSolarConstants.FacilitiesProp];
                var locationQuery = BaseQueryUtil.GenerateOrLikeString("workorder.location", facilities.Select(f => f + "%"),true);
                sb.AppendFormat(" and ({0})", locationQuery);
            }
            return sb.ToString();
        }

        private string BaseFacilityQuery(string columnName) {
            var user = SecurityFacade.CurrentUser();
            var sb = new StringBuilder();
            if (user.Genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                var facilities = (IEnumerable<string>)user.Genericproperties[FirstSolarConstants.FacilitiesProp];
                var locationQuery = BaseQueryUtil.GenerateOrLikeString(columnName, facilities.Select(f => f + "%"),true);
                sb.AppendFormat("({0})", locationQuery);
            }
            return sb.ToString();
        }

        public string LocationWhereClauseByFacility() {
            return BaseFacilityQuery("location.location");
        }

        public string AssetWhereClauseByFacility() {
            return BaseFacilityQuery("asset.location");
        }

        public string LocAncestorWhereClauseByFacility() {
            return BaseFacilityQuery("locancestor.ancestor");
        }

    }
}
