using System.Collections.Generic;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.dashboard.classes.startup;
using System.ComponentModel.Composition;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using log4net;
using NHibernate.Mapping.ByCode;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Configuration.Services.Api;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dashboard {
    public class FirstSolarDashboardInitializer : ISWEventListener<ApplicationStartedEvent>, IOrdered {


        public const string TechnicianMyAssignmentsDash = "fs.techmyassignments";

        public const string TodayPanel = "fs.myassignments.today";
        public const string PastPanel = "fs.myassignments.past";
        public const string FuturePanel = "fs.myassignments.future";
        public const string MyAssignmentsAllPanel = "fs.myassignments.all";

        public const string TechnicianGroupAssignmentDash = "fs.groupassignments";

        public const string ScheduledPanel = "fs.groupassignments.scheduled";
        public const string PlannedNotScheduledPanel = "fs.groupassignments.pnscheduled";
        public const string NPlannedNotScheduledPanel = "fs.groupassignments.npnscheduled";
        public const string AssignedToOthersPanel = "fs.groupassignments.assignothers";



        public const string CorrectiveMaintenanceDashAlias = "fs.cmmaintenance";
        public const string PreventiveMaintenanceDashAlias = "fs.pmmaintenance";


        public const string MaintenanceDashCmAlias = "fs.maintenancecm";

        public const string IncomingPanelAlias = "fs.maintenance.incoming";
        public const string IncomingPanelSchemaId = "workpackageincomingdash";

        public const string IncomingPanelCmAlias = "fs.maintenancecm.incoming";
        public const string IncomingCmPanelSchemaId = "workpackageincomingcmdash";

        public const string BuildPanelAlias = "fs.maintenance.build";
        public const string BuildPanelAlias290PM = "fs.maintenance.290pm";
        public const string BuildPanelAlias290CM = "fs.maintenance.290cm";

        public const string PmBuildPanelSchemaId = "pmbuilddash";
        public const string CmBuildPanelSchemaId = "cmbuilddash";


        private ILog Log = LogManager.GetLogger(typeof(FirstSolarDashboardInitializer));

        [Import]
        public DashboardInitializationService DashboardInitializationService { get; set; }

        [Import]
        public IWhereClauseFacade WhereClauseFacade { get; set; }

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public FirstSolarDashboardWcBuilder FSDashboardWcBuilder { get; set; }

        [Transactional(DBType.Swdb)]
        public virtual void HandleEvent(ApplicationStartedEvent eventToDispatch) {

            Log.Info("initing fs dashboard registry");

            DashboardInitializationService.Inactivate("fs.maintenance");

            var cmDash = DashboardInitializationService.FindByAlias(CorrectiveMaintenanceDashAlias) ??
                DashboardInitializationService.CreateDashboard("Corrective Maintenance", CorrectiveMaintenanceDashAlias, new List<DashboardBasePanel>());

            var myaDash = DashboardInitializationService.FindByAlias(TechnicianMyAssignmentsDash) ??
                DashboardInitializationService.CreateDashboard("My Assignments", TechnicianMyAssignmentsDash, new List<DashboardBasePanel>());

            var groupaDash = DashboardInitializationService.FindByAlias(TechnicianGroupAssignmentDash) ??
                          DashboardInitializationService.CreateDashboard("Group Assignments", TechnicianGroupAssignmentDash, new List<DashboardBasePanel>());

            var pmDash = DashboardInitializationService.FindByAlias(PreventiveMaintenanceDashAlias) ??
                                  DashboardInitializationService.CreateDashboard("Preventive Maintenance", PreventiveMaintenanceDashAlias, new List<DashboardBasePanel>(), 1);

            cmDash.Application = "_WorkPackage";
            pmDash.Application = "_WorkPackage";

            //so that they are visible to workorder allowed users
            //TODO: security group dashboard selection
            myaDash.Application = "workorder";
            groupaDash.Application = "workorder";

            pmDash.PreferredOrder = 1;
            cmDash.PreferredOrder = 2;

            myaDash.PreferredOrder = 3;
            groupaDash.PreferredOrder = 4;


            Dao.Save(cmDash);
            Dao.Save(pmDash);


            #region CM
            var panelsCM = new List<DashboardBasePanel> {
                new DashboardGridPanel
                {
                    Alias = BuildPanelAlias290CM,
                    Title = "Work Package Queue",
                    Application = "workorder",
                    AppFields = "#wpnum,description,planner,facilityname,asset_.description,reportdate,daysleftcm,#buildcomplete,#colorcode",
                    DefaultSortField = "reportdate desc",
                    SchemaRef = CmBuildPanelSchemaId,
                    Limit = 10,
                    Size = 12
                },

                new DashboardGridPanel
                {
                    Alias = IncomingPanelCmAlias,
                    Title = "Incoming Maximo Work Order Queue",
                    Application = "workorder",
                    AppFields = "wonum,description,planner,facilityname,asset_.description,reportdate",
                    DefaultSortField = "reportdate desc",
                    SchemaRef = IncomingCmPanelSchemaId,
                    Limit = 10,
                    Size = 12
                },
            };
            DashboardInitializationService.RegisterWhereClause("workorder", FSDashboardWcBuilder.MaintenanceDashBuild290CMQuery(), "WoMaintenananceBuild290CM", "dashboard:" + BuildPanelAlias290CM);
            DashboardInitializationService.RegisterWhereClause("workorder", "@firstSolarDashboardWcBuilder.MaintenanceCmDashQuery", "WoMaintenananceCmIncoming", "dashboard:" + IncomingPanelCmAlias);
            DashboardInitializationService.AddPanelsToDashboard(cmDash, panelsCM);
            #endregion

            #region PM
            var panelsPM = new List<DashboardBasePanel>{
                new DashboardGridPanel
                {
                    Alias = BuildPanelAlias290PM,
                    Title = "Today",
                    Application = "workorder",
                    AppFields = "#wpnum,description,planner,facilityname,asset_.description,reportdate,schedstart,daysleft,#buildcomplete,#colorcode",
                    DefaultSortField = "daysleft",
                    SchemaRef = PmBuildPanelSchemaId,
                    Limit = 10,
                    Size = 12
                },
                new DashboardGridPanel
                {
                    Alias = BuildPanelAlias,
                    Title = "Build Queue (54-30 Days)",
                    Application = "workorder",
                    AppFields = "#wpnum,description,planner,facilityname,asset_.description,reportdate,schedstart,daysleft,#buildcomplete,#colorcode",
                    DefaultSortField = "daysleft",
                    SchemaRef = PmBuildPanelSchemaId,
                    Limit = 10,
                    Size = 12
                },
                new DashboardGridPanel
                {
                    Alias = IncomingPanelAlias,
                    Title = "Incoming Queue PM (55+ Days)",
                    Application = "workorder",
                    AppFields = "wonum,description,planner,facilityname,asset_.description,reportdate,schedstart,daysleft",
                    DefaultSortField = "daysleft",
                    SchemaRef = IncomingPanelSchemaId,
                    Limit = 10,
                    Size = 12
                }
            };
            DashboardInitializationService.RegisterWhereClause("workorder", FSDashboardWcBuilder.MaintenanceDashBuild290PMQuery(), "WoMaintenananceBuild290PM", "dashboard:" + BuildPanelAlias290PM);
            DashboardInitializationService.RegisterWhereClause("workorder", FSDashboardWcBuilder.MaintenanceDashBuild30PQuery(), "WoMaintenananceBuild", "dashboard:" + BuildPanelAlias);
            DashboardInitializationService.RegisterWhereClause("workorder", "@firstSolarDashboardWcBuilder.MaintenanceDashQuery", "WoMaintenananceIncoming", "dashboard:" + IncomingPanelAlias);
            DashboardInitializationService.AddPanelsToDashboard(pmDash, panelsPM);
            #endregion


            #region AssignedToMe
            var panelsTechMine = new List<DashboardBasePanel> {


                new DashboardGridPanel
                {
                    Alias = TodayPanel,
                    Title = "Today",
                    Application = "assignment",
                    DefaultSortField = "scheduledate",
                    SchemaRef = "techdashboard",
                    Limit = 10,
                    Size = 6
                },

                new DashboardGridPanel
                {
                    Alias = PastPanel,
                    Title = "Past",
                    Application = "assignment",
                    DefaultSortField = "scheduledate",
                    SchemaRef = "techdashboard",
                    Limit = 10,
                    Size = 6
                },

                new DashboardGridPanel
                {
                    Alias = FuturePanel,
                    Title = "Future",
                    Application = "assignment",
                    DefaultSortField = "scheduledate",
                    SchemaRef = "techdashboard",
                    Limit = 10,
                    Size = 6
                },

                new DashboardGridPanel
                {
                    Alias = MyAssignmentsAllPanel,
                    Title = "All",
                    Application = "assignment",
                    DefaultSortField = "scheduledate",
                    SchemaRef = "techdashboard",
                    Limit = 10,
                    Size = 6
                },


            };
            DashboardInitializationService.AddPanelsToDashboard(myaDash, panelsTechMine);
//
            DashboardInitializationService.RegisterWhereClause("assignment", "@firstSolarWhereClauseRegistry.TodayWhereClauseForDashMethod", "TodayPanel", "dashboard:" + TodayPanel);
            DashboardInitializationService.RegisterWhereClause("assignment", "@firstSolarWhereClauseRegistry.PastWhereClauseForDashMethod", "PastPanel", "dashboard:" + PastPanel);
            DashboardInitializationService.RegisterWhereClause("assignment", "@firstSolarWhereClauseRegistry.FutureWhereClauseForDashMethod", "FuturePanel", "dashboard:" + FuturePanel);
            DashboardInitializationService.RegisterWhereClause("assignment", "@firstSolarWhereClauseRegistry.AllWhereClauseForDashMethod", "AllPanel", "dashboard:" + MyAssignmentsAllPanel);


            #endregion


            #region Group
            var panelsTechGroup = new List<DashboardBasePanel> {


                new DashboardGridPanel
                {
                    Alias = ScheduledPanel,
                    Title = "Scheduled",
                    Application = "assignment",
                    DefaultSortField = "scheduledate",
                    SchemaRef = "techdashboard",
                    Limit = 10,
                    Size = 6
                },

                new DashboardGridPanel
                {
                    Alias = PlannedNotScheduledPanel,
                    Title = "Planned Not Scheduled",
                    Application = "assignment",
                    DefaultSortField = "scheduledate",
                    SchemaRef = "techdashboard",
                    Limit = 10,
                    Size = 6
                },

                new DashboardGridPanel
                {
                    Alias = NPlannedNotScheduledPanel,
                    Title = "Not Planned Not Scheduled",
                    Application = "assignment",
                    DefaultSortField = "scheduledate",
                    SchemaRef = "techdashboard",
                    Limit = 10,
                    Size = 6
                },

                new DashboardGridPanel
                {
                    Alias = AssignedToOthersPanel,
                    Title = "Assigned to Others",
                    Application = "assignment",
                    DefaultSortField = "scheduledate",
                    SchemaRef = "techdashboard",
                    Limit = 10,
                    Size = 6
                },





            };

            DashboardInitializationService.RegisterWhereClause("assignment", "@firstSolarWhereClauseRegistry.SchedWhereClauseForDashMethod", "SchedPanel", "dashboard:" + ScheduledPanel);
            DashboardInitializationService.RegisterWhereClause("assignment", "@firstSolarWhereClauseRegistry.PNSchedWhereClauseForDashMethod", "PNSchedPanel", "dashboard:" + PlannedNotScheduledPanel);
            DashboardInitializationService.RegisterWhereClause("assignment", "@firstSolarWhereClauseRegistry.NPNSchedWhereClauseForDashMethod", "NPNSchedPanel", "dashboard:" + NPlannedNotScheduledPanel);
            DashboardInitializationService.RegisterWhereClause("assignment", "@firstSolarWhereClauseRegistry.OtherWhereClauseForDashMethod", "OtherPanel", "dashboard:" + AssignedToOthersPanel);

            DashboardInitializationService.AddPanelsToDashboard(groupaDash, panelsTechGroup);

            #endregion



            Log.Info("finishing dashboard registry");
        }

        public int Order => ChartInitializer.ORDER + 1;
    }
}
