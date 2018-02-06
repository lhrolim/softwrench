using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.util;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration {
    public class FirstSolarWhereClauseRegistry : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {


        private readonly IWhereClauseFacade _whereClauseFacade;
        private FirstSolarFacilityUtil _firstSolarFacilityUtil;
        private IContextLookuper _contextLookuper;

        /// <summary>
        /// Brings all assignments, where exists a workorder of interest, narrowing by the facility query that will be replaced at {0}
        /// no server side filtering based on labor code should take place since these would happen at client side
        /// </summary>
        private const string AssignedWhereClause =
            @"exists 
            (select 1 from workorder as workorder_ where workorder_.wonum = assignment.wonum and workorder_.siteid = assignment.siteid and workorder_.orgid = assignment.orgid and 
                workorder_.status not in ('comp','can','close') and workorder_.status in ('APPR','INPRG','WAPPR') and historyflag = 0 and istask = 0 and ({0}) )";  /// <summary>


        //        /// Brings all assignments, where exists a workorder of interest, narrowing by the facility query that will be replaced at {0}
        //        /// no server side filtering based on labor code should take place since these would happen at client side
        //        /// </summary>
        //        private const string AssignedWhereClause2 =
        //            @"assignment.assignmentid in 
        //            (select assignmentid from assignment assignment_ inner join workorder workorder_ on (workorder_.wonum = assignment_.wonum and workorder_.siteid = assignment_.siteid and workorder_.orgid = assignment_.orgid)
        //                where workorder_.wonum is not null  and workorder_.status not in ('comp','can','close')
        //                and workorder_.status in ('APPR','INPRG','WAPPR') and workorder_.historyflag = 0 and workorder_.istask = 0 and {0} )";


        private const string WOAssignedWhereClause =
        @"workorder.status not in ('comp','can','close') and workorder.status in ('APPR','INPRG','WAPPR') and workorder.siteid :siteid and historyflag = 0 and istask = 0
            and 
            exists (select 1 from assignment a where workorder.wonum = a.wonum and workorder.siteid = a.siteid and workorder.orgid = a.orgid 
                and a.laborcode = '@user.properties['laborcode']')
          ";

        /// <summary>
        /// Brings all workorders excluding the ones which already have an assignment for the current user on the week (but that have at least one assignment for other users).
        ///  Facility filters will be applied on top of this to narrow the list
        /// </summary>
        private const string WOGroupByBaseWhereClause =
          @"workorder.status not in ('comp','can','close') and workorder.status in ('APPR','INPRG','WAPPR') and workorder.siteid :siteid and historyflag = 0 and istask = 0
            and {0}
            and not
            exists (select 1 from assignment a where workorder.wonum = a.wonum and workorder.siteid = a.siteid and workorder.orgid = a.orgid 
                and a.laborcode = '@user.properties['laborcode']')
            and exists (select 1 from assignment a where workorder.wonum = a.wonum and workorder.siteid = a.siteid and workorder.orgid = a.orgid 
                and (a.laborcode != '@user.properties['laborcode']' or a.laborcode is null))";

        /// <summary>
        /// Brings all workorders where there´s not a single assignment created for it
        /// </summary>
        private const string UnassignedWhereClause =
                @"workorder.status not in ('comp','can','close') and workorder.status in ('APPR','INPRG','WAPPR') and workorder.siteid :siteid and historyflag = 0 and istask = 0
            and {0}
            and not
            exists (select 1 from assignment a where workorder.wonum = a.wonum and workorder.siteid = a.siteid and workorder.orgid = a.orgid)";


        private const string TodayWhereClause =
                @"(workorder.siteid in ('1803', '1808', '1801', '4801')) and workorder.status not in ('MISSD','COMP','COMP-PEND','CAN','CLOSE') and ({0})
                and istask = 0 and historyflag = 0 and worktype is not null and workorder.wonum in (select assignment.wonum from assignment where workorder.wonum=assignment.wonum and workorder.orgid=assignment.orgid 
                and assignment.status='ASSIGNED' and cast (assignment.scheduledate as date) = cast (getdate() as date) and assignment.laborcode in (select labor.laborcode from labor where labor.personid= @personid))";


        #region dashwhereclauses

        private const string TodayDashWhereClause =
            @"(workorder.siteid in ('1803', '1808', '1801', '4801')) and workorder.status not in ('MISSD','COMP','COMP-PEND','CAN','CLOSE') and ({0})
                and istask = 0 and historyflag = 0 and worktype is not null and 
                assignment.status='ASSIGNED' and cast (assignment.scheduledate as date) = cast (getdate() as date) and assignment.laborcode = '@user.properties['laborcode']'";


        /// <summary>
        /// USed exclusively for dashboards, refers to assignment entity
        /// </summary>
        private const string PastMyWhereClauseForDashboard =
            @"workorder.status not in ('comp','can','close') and workorder.status in ('APPR','INPRG','WAPPR') and workorder.siteid :siteid and historyflag = 0 and istask = 0
            and cast (assignment.scheduledate as date) < cast (getdate() as date)
            and assignment.laborcode = '@user.properties['laborcode']'";


        private const string FutureDashMyWhereClause =
            @"workorder.status not in ('comp','can','close') and workorder.status in ('APPR','INPRG','WAPPR') and workorder.siteid :siteid and historyflag = 0 and istask = 0
            and cast (assignment.scheduledate as date) > cast (getdate() as date) and assignment.laborcode = '@user.properties['laborcode']'";

        private const string AssignedDashWhereClause =
            @"workorder.status not in ('comp','can','close') and workorder.status in ('APPR','INPRG','WAPPR') and workorder.siteid :siteid and historyflag = 0 and istask = 0
            and assignment.laborcode = '@user.properties['laborcode']'
          ";

        #endregion



        private const string PastWhereClause =
            @"woeq4 = 1 
                and onmstatusflag = 0 
                and istask = 0 
                and historyflag = 0
                and workorder.status not in ('ENRV') and ({0})
                and schedstart is not null 
                and worktype is not null 
                and workorder.persongroup in (select persongroupteam.persongroup from persongroupteam where persongroupteam.respparty= @personid ) 
                and (select case when min(cast(assignment.scheduledate as date)) is null and min(cast(assignment.startdate as date)) is null then cast(workorder.schedstart as date) 
                when min(cast(assignment.scheduledate as date)) is not null and min(cast(assignment.startdate as date)) is null then min(cast(assignment.scheduledate as date)) 
                when min(cast(assignment.scheduledate as date)) is null and min(cast(assignment.startdate as date)) is not null then min(cast(assignment.startdate as date)) 
                when min(cast(assignment.scheduledate as date))<= min(cast(assignment.startdate as date)) then min(cast(assignment.scheduledate as date)) 
                when min(cast(assignment.scheduledate as date))> min(cast(assignment.startdate as date)) then min(cast(assignment.startdate as date)) end 
                from assignment 
                where workorder.wonum=assignment.wonum and workorder.siteid = assignment.siteid )< cast(getdate() as date) ";


        /// <summary>
        /// Scheduled Panel
        /// </summary>
        private const string SchedWhereClause =
            @"workorder.status not in ('MISSD','COMP-PEND','COMP','CAN','CLOSE','ENRV') 
              and istask = 0 
              and schedstart is not null 
              and worktype is not null 
              and historyflag = 0
              and ({0})
              and workorder.persongroup in (select persongroupteam.persongroup from persongroupteam where persongroupteam.respparty= @personid ) 
              and (select case when max(cast(assignment.scheduledate as date)) is null and max(cast(assignment.startdate as date)) is null then cast(workorder.schedstart as date) 
              when max(cast(assignment.scheduledate as date)) is not null and max(cast(assignment.startdate as date)) is null then max(cast(assignment.scheduledate as date)) 
              when max(cast(assignment.scheduledate as date)) is null and max(cast(assignment.startdate as date)) is not null then max(cast(assignment.startdate as date)) 
              when max(cast(assignment.scheduledate as date))>= max(cast(assignment.startdate as date)) then max(cast(assignment.scheduledate as date)) 
              when max(cast(assignment.scheduledate as date))< max(cast(assignment.startdate as date)) then max(cast(assignment.startdate as date)) end 
              from assignment where workorder.wonum=assignment.wonum and workorder.siteid = assignment.siteid )>=cast(getdate() as date)";

        /// <summary>
        ///  Planned not Scheduled Panel
        /// </summary>
        private const string PnSchedWhereClause =
            @"woeq4 = 1 and onmstatusflag = 0 and istask = 0 
            and (wolo10 >= 90 or wolo10 is null) 
            and worktype is not null 
            and historyflag = 0
            and ({0})
            and (select distinct assignment.wonum from assignment where assignment.wonum= workorder.wonum and assignment.siteid = workorder.siteid and assignment.craft is not null) = workorder.wonum 
            and wopriority is not null 
            and (select sum(laborhrs) from assignment where assignment.wonum= workorder.wonum and assignment.siteid = workorder.siteid ) is not null 
            and schedstart is null 
            and workorder.persongroup in (select persongroupteam.persongroup from persongroupteam where persongroupteam.respparty= @personid )";

        /// <summary>
        ///  Planned not Scheduled Panel
        /// </summary>
        private const string NPnSchedWhereClause =
            @"woeq4 = 1 
            and onmstatusflag = 0 
            and istask = 0 
            and historyflag = 0
            and (worktype in ('EM','FIN','AD') or reportedby = @personid ) 
            and not(((select distinct assignment.wonum from assignment where assignment.wonum= workorder.wonum and assignment.siteid = workorder.siteid and assignment.craft is not null) = workorder.wonum) 
            and (wopriority is not null) 
            and ((select sum(laborhrs) from assignment where assignment.wonum= workorder.wonum and assignment.siteid = workorder.siteid ) is not null)) 
            and schedstart is null 
            and workorder.persongroup in (select persongroupteam.persongroup from persongroupteam where persongroupteam.respparty= @personid )";

        private const string AssetQuery = @"(asset.status in ('ACTIVE', 'OPERATING')) and ({0})";


        private const string UserLaborWhereClause = "labor.laborcode='@user.properties['laborcode']' and labor.orgid=@orgid";
        private const string UserLaborCraftWhereClause = "laborcraftrate.laborcode='@user.properties['laborcode']' and laborcraftrate.orgid=@orgid";


        public FirstSolarWhereClauseRegistry(IWhereClauseFacade whereClauseFacade, FirstSolarFacilityUtil firstSolarFacilityUtil, IContextLookuper contextLookuper) {
            _whereClauseFacade = whereClauseFacade;
            _firstSolarFacilityUtil = firstSolarFacilityUtil;
            _contextLookuper = contextLookuper;
        }

        [Transactional(DBType.Swdb)]
        public virtual void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (!ApplicationConfiguration.IsClient("firstsolar")) {
                //to avoid issues on dev environments
                return;
            }

            var offLineCondition = new WhereClauseRegisterCondition() { Alias = "offline", OfflineOnly = true, Global = true };
            var techWorkorderCondition = new WhereClauseRegisterCondition { Alias = "techworkorder", Global = true, AppContext = new ApplicationLookupContext { ParentApplication = "workorder" } };
            // right now there is not matusetrans on fsoc workorders, so parent app matusetrans is enough to filter for techs
            var techMatusetransCondition = new WhereClauseRegisterCondition { Alias = "techworkorder", Global = true, AppContext = new ApplicationLookupContext { ParentApplication = "matusetrans" } };

            var fsocWorkorderCondition = new WhereClauseRegisterCondition { Alias = "fsocworkorder", Global = true, AppContext = new ApplicationLookupContext { ParentApplication = "fsocworkorder" } };

            _whereClauseFacade.Register("workorder", "@firstSolarWhereClauseRegistry.AssignedByGroup", offLineCondition);
            _whereClauseFacade.Register("schedworkorder", "@firstSolarWhereClauseRegistry.SchedWhereClauseMethod", offLineCondition);
            _whereClauseFacade.Register("todayworkorder", "@firstSolarWhereClauseRegistry.TodayWhereClauseMethod", offLineCondition);
            _whereClauseFacade.Register("pastworkorder", "@firstSolarWhereClauseRegistry.PastWhereClauseMethod", offLineCondition);
            _whereClauseFacade.Register("pnschedworkorder", "@firstSolarWhereClauseRegistry.PnSchedWhereClauseMethod", offLineCondition);
            _whereClauseFacade.Register("npnschedworkorder", "@firstSolarWhereClauseRegistry.NPnSchedWhereClauseMethod", offLineCondition);

            _whereClauseFacade.Register("otherworkorder", "@firstSolarWhereClauseRegistry.WorkordersByGroup", offLineCondition);
            _whereClauseFacade.Register("otherworkorderunassigned", "@firstSolarWhereClauseRegistry.UnassignedWorkorder", offLineCondition);


            _whereClauseFacade.Register("assignment", "@firstSolarWhereClauseRegistry.AssignmentsByGroup", offLineCondition);
            _whereClauseFacade.Register("assignment", "@firstSolarWhereClauseRegistry.AssignmentsByGroup", offLineCondition);
            _whereClauseFacade.Register("classstructure", "useclassindesc= 1 and parent is null and genassetdesc =0 and siteid is null and type is null and haschildren=0 and showinassettopo = 1", offLineCondition);




            _whereClauseFacade.Register("offlinelocation", "@firstSolarWhereClauseRegistry.LocationWhereClauseByFacility", offLineCondition);
            _whereClauseFacade.Register("offlineasset", "@firstSolarWhereClauseRegistry.AssetWhereClauseByFacility", offLineCondition);

            _whereClauseFacade.Register("locancestor", "@firstSolarWhereClauseRegistry.LocAncestorWhereClauseByFacility", offLineCondition);
            _whereClauseFacade.Register("offlineinventory", "@firstSolarWhereClauseRegistry.InventoryWhereClauseByFacility", offLineCondition);
            _whereClauseFacade.Register("offlineitem", "@firstSolarWhereClauseRegistry.ItemWhereClauseByFacility", offLineCondition);
            _whereClauseFacade.Register("synstatus", "value in ('WOEN','COMP')", offLineCondition);
            _whereClauseFacade.Register("labor", UserLaborWhereClause, offLineCondition);
            _whereClauseFacade.Register("laborcraftrate", UserLaborCraftWhereClause, offLineCondition);


            _whereClauseFacade.Register("location", "@firstSolarWhereClauseRegistry.LocationWhereClauseByFacility", techWorkorderCondition);
            _whereClauseFacade.Register("location", "@firstSolarWhereClauseRegistry.LocationWhereClauseByFacility", techMatusetransCondition);
            _whereClauseFacade.Register("asset", "@firstSolarWhereClauseRegistry.AssetWhereClauseByFacility", techWorkorderCondition);
            _whereClauseFacade.Register("asset", "@firstSolarWhereClauseRegistry.AssetWhereClauseByFacility", fsocWorkorderCondition);
            _whereClauseFacade.Register("inventory", "@firstSolarWhereClauseRegistry.InventoryWhereClauseByFacility", techWorkorderCondition);

            var multiAssetCondition = new WhereClauseRegisterCondition { Alias = "multiasset", Global = true };
            multiAssetCondition = multiAssetCondition.AppendSchema("multiAssetLookupList");
            _whereClauseFacade.Register("asset", "@firstSolarWhereClauseRegistry.AssetWhereClauseByFacility", multiAssetCondition);
        }


        public string TodayWhereClauseMethod() {
            return DoBuildQuery(TodayWhereClause, "workorder.location");
        }


        #region MyAssignmentsDash

        public string TodayWhereClauseForDashMethod() {
            return DoBuildQuery(TodayDashWhereClause.Replace("workorder.", "workorder_."), "workorder_.location");
        }

        public string PastWhereClauseForDashMethod() {
            return DoBuildQuery(PastMyWhereClauseForDashboard.Replace("workorder", "workorder_"), "workorder_.location");
        }

        public string FutureWhereClauseForDashMethod() {
            return DoBuildQuery(FutureDashMyWhereClause.Replace("workorder", "workorder_"), "workorder_.location");
        }

        public string AllWhereClauseForDashMethod() {
            return DoBuildQuery(AssignedDashWhereClause.Replace("workorder", "workorder_"), "workorder_.location");
        }

        #endregion

        #region OtherDash

        public string SchedWhereClauseForDashMethod() {
            return DoBuildQuery(SchedWhereClause.Replace("workorder.", "workorder_."), "workorder_.location");
        }

        public string PNSchedWhereClauseForDashMethod() {
            return DoBuildQuery(PnSchedWhereClause.Replace("workorder", "workorder_"), "workorder_.location");
        }

        public string NPNSchedWhereClauseForDashMethod() {
            return DoBuildQuery(NPnSchedWhereClause.Replace("workorder", "workorder_"), "workorder_.location");
        }

        public string OtherWhereClauseForDashMethod() {
            return DoBuildQuery(WOGroupByBaseWhereClause.Replace("workorder", "workorder_"), "workorder_.location");
        }

        public string UnassignedClauseForDashMethod() {
            return DoBuildQuery(UnassignedWhereClause, "workorder.location");
        }

        #endregion


        public string PastWhereClauseMethod() {
            return DoBuildQuery(PastWhereClause, "workorder.location");
        }

        public string SchedWhereClauseMethod() {
            return DoBuildQuery(SchedWhereClause, "workorder.location");
        }

        public string PnSchedWhereClauseMethod() {
            return DoBuildQuery(PnSchedWhereClause, "workorder.location");
        }

        public string NPnSchedWhereClauseMethod() {
            return DoBuildQuery(NPnSchedWhereClause, "workorder.location");
        }


        public string AssignmentsByGroup() {
            return DoBuildQuery(AssignedWhereClause, "workorder_.location");
        }

        public string WorkordersByGroup() {
            return DoBuildQuery(WOGroupByBaseWhereClause, "workorder.location");
        }

        public string UnassignedWorkorder() {
            return DoBuildQuery(UnassignedWhereClause, "workorder.location");
        }

        public string Fr1CodeQuery(string context) {
            return _contextLookuper.LookupContext().OfflineMode ? "(select failurecode from failurereport fr where fr.wonum = workorder.wonum and fr.type = 'CAUSE' and fr.orgid = workorder.orgid and fr.siteid = workorder.siteid)" : "null";
        }

        public string Fr2CodeQuery(string context) {
            return _contextLookuper.LookupContext().OfflineMode ? " (select failurecode from failurereport fr where fr.wonum = workorder.wonum and fr.type='REMEDY' and fr.orgid = workorder.orgid and fr.siteid = workorder.siteid)" : "null";
        }

        public string AssignedByGroup() {
            var user = SecurityFacade.CurrentUser();
            var sb = new StringBuilder();

            var baseQuery = WOAssignedWhereClause;

            baseQuery = ReplaceSiteId(user, baseQuery);


            sb.Append(DefaultValuesBuilder.ConvertAllValues(baseQuery, user));
            if (user.Genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                var facilities = (IEnumerable<string>)user.Genericproperties[FirstSolarConstants.FacilitiesProp];
                var locationQuery = BaseQueryUtil.GenerateOrLikeString("workorder.location", facilities.Select(f => f + "%"), true);
                sb.AppendFormat(" and ({0})", locationQuery);
            }
            return sb.ToString();
        }

        //TODO: move to default valuesbuilder
        private static string ReplaceSiteId(InMemoryUser user, string baseQuery) {
            var secondSite = user.UserPreferences?.GetGenericProperty(FirstSolarConstants.SecondarySite)?.Value;
            var sites = new List<string> { user.SiteId };
            if (!string.IsNullOrEmpty(secondSite)) {
                sites.Add(secondSite);
            }
            if (sites.Count > 1) {
                baseQuery = baseQuery.Replace(":siteid", $" in ({BaseQueryUtil.GenerateInString(sites)})");
            } else {
                //using = in order to keep the 99% scenario faster
                baseQuery = baseQuery.Replace(":siteid", $" = '{user.SiteId}'");
            }
            return baseQuery;
        }


        private string DoBuildQuery(string queryToUse, string columnName) {
            var user = SecurityFacade.CurrentUser();
            var locationQuery = _firstSolarFacilityUtil.BaseFacilityQuery(columnName);
            var baseQuery = queryToUse.Fmt(locationQuery);

            baseQuery = ReplaceSiteId(user, baseQuery);



            return DefaultValuesBuilder.ConvertAllValues(baseQuery, user);
        }

        public string LocationWhereClauseByFacility() {
            var user = SecurityFacade.CurrentUser();
            if (!user.Genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                return "(1!=1)";
            }
            var byFacility = _firstSolarFacilityUtil.BaseFacilityQuery("location.location");
            var byStoreRoomFAcility = _firstSolarFacilityUtil.BaseStoreroomFacilityQuery("d.ldtext");
            return $"({byFacility} or (location.type = 'storeroom' and location.location in " +
                   $"( select location.location from locations location inner join longdescription d on (location.locationsid = d.ldkey and ldownertable = 'LOCATIONS')" +
                   $" and {byStoreRoomFAcility})))";
        }

        public string AssetWhereClauseByFacility() {
            return DoBuildQuery(AssetQuery, "asset.location");
        }

        public string LocAncestorWhereClauseByFacility() {
            return _firstSolarFacilityUtil.BaseFacilityQuery("locancestor.ancestor");
        }

        private string BaseSelectFromStoreroom() {
            var byFacility = _firstSolarFacilityUtil.BaseFacilityQuery("location.location");
            var byStoreRoomFAcility = _firstSolarFacilityUtil.BaseStoreroomFacilityQuery("d.ldtext");
            return $"select location.location from locations location inner join longdescription d on (location.locationsid = d.ldkey and ldownertable = 'LOCATIONS') where location.type = 'storeroom' and (({byFacility}) or ({byStoreRoomFAcility}))";
        }

        public string InventoryWhereClauseByFacility() {
            var user = SecurityFacade.CurrentUser();
            if (!user.Genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                return "(1!=1)";
            }
            return $"( inventory.location in ({BaseSelectFromStoreroom()}))";
        }

        public string ItemWhereClauseByFacility() {
            var user = SecurityFacade.CurrentUser();
            if (!user.Genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                return "(1!=1)";
            }
            return $"offlineitem.itemtype = 'ITEM' and offlineitem.status = 'ACTIVE' and offlineitem.rotating = 0 and offlineitem.itemnum in (select inventory.itemnum from inventory inventory where inventory.location in ({BaseSelectFromStoreroom()}))";
        }
    }
}