using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration {
    public class FirstSolarWhereClauseRegistry : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {


        private readonly IWhereClauseFacade _whereClauseFacade;

        private const string WOAssignedWhereClause =
            @"workorder.status not in ('comp','can','close') and workorder.siteid = @siteid
            and 
            exists (select 1 from assignment a where workorder.wonum = a.wonum and workorder.siteid = a.siteid and workorder.orgid = a.orgid 
                and a.laborcode = '@user.properties['laborcode']' and a.scheduledate >= @past(1week) and a.scheduledate <= @future(1week))";


        private const string WOGroupByBaseWhereClause =
          @"workorder.status not in ('comp','can','close') and workorder.siteid = @siteid
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

            var offLineCondition = new WhereClauseRegisterCondition() { Alias = "offline",OfflineOnly = true, Global = true};

            _whereClauseFacade.Register("workorder", WOAssignedWhereClause, offLineCondition);
            _whereClauseFacade.Register("otherworkorder", "@firstSolarWhereClauseRegistry.WorkordersByGroup", offLineCondition);
            _whereClauseFacade.Register("offlinelocation", "@firstSolarWhereClauseRegistry.LocationWhereClauseByFacility", offLineCondition);
            _whereClauseFacade.Register("offlineasset", "@firstSolarWhereClauseRegistry.AssetWhereClauseByFacility", offLineCondition);
            _whereClauseFacade.Register("locancestor", "@firstSolarWhereClauseRegistry.LocAncestorWhereClauseByFacility", offLineCondition);
            _whereClauseFacade.Register("synstatus", "value in ('WOEN','COMP')", offLineCondition);
            _whereClauseFacade.Register("labor", UserLaborWhereClause, offLineCondition);
            _whereClauseFacade.Register("laborcraftrate", UserLaborCraftWhereClause, offLineCondition);
        }

        public string WorkordersByGroup() {
            var user = SecurityFacade.CurrentUser();
            var sb = new StringBuilder();
            sb.Append(DefaultValuesBuilder.ConvertAllValues(WOGroupByBaseWhereClause, user));
            if (user.Genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                var facilities = (IEnumerable<string>)user.Genericproperties[FirstSolarConstants.FacilitiesProp];
                var locationQuery = BaseQueryUtil.GenerateOrLikeString("workorder.location", facilities.Select(f => f + "%"));
                sb.AppendFormat(" and ({0})", locationQuery);
            }
            return sb.ToString();
        }

        private string BaseFacilityQuery(string columnName) {
            var user = SecurityFacade.CurrentUser();
            var sb = new StringBuilder();
            if (user.Genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                var facilities = (IEnumerable<string>)user.Genericproperties[FirstSolarConstants.FacilitiesProp];
                var locationQuery = BaseQueryUtil.GenerateOrLikeString(columnName, facilities.Select(f => f + "%"));
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
