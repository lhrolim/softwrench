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

            var pmDash = DashboardInitializationService.FindByAlias(PreventiveMaintenanceDashAlias) ??
                                  DashboardInitializationService.CreateDashboard("Preventive Maintenance", PreventiveMaintenanceDashAlias, new List<DashboardBasePanel>(),1);

            cmDash.Application = "_WorkPackage";
            pmDash.Application = "_WorkPackage";
            pmDash.PreferredOrder = 1;


            Dao.Save(cmDash);
            Dao.Save(pmDash);


            var panelsCM = new List<DashboardBasePanel> {
                new DashboardGridPanel
                {
                    Alias = BuildPanelAlias290CM,
                    Title = "Work Package Queue",
                    Application = "workorder",
                    AppFields = "#wpnum,description,planner,location_.facilityname,asset_.description,reportdate,schedstart,daysleftcm,#buildcomplete,#colorcode",
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
                    AppFields = "wonum,description,planner,location_.facilityname,asset_.description,reportdate,schedstart",
                    DefaultSortField = "schedstart",
                    SchemaRef = IncomingCmPanelSchemaId,
                    Limit = 10,
                    Size = 12
                },
            };


            var panelsPM = new List<DashboardBasePanel> {


                new DashboardGridPanel
                {
                    Alias = BuildPanelAlias290PM,
                    Title = "29-0 Days Queue",
                    Application = "workorder",
                    AppFields = "#wpnum,description,planner,location_.facilityname,asset_.description,reportdate,schedstart,daysleft,#buildcomplete,#colorcode",
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
                    AppFields = "#wpnum,description,planner,location_.facilityname,asset_.description,reportdate,schedstart,daysleft,#buildcomplete,#colorcode",
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
                    AppFields = "wonum,description,planner,location_.facilityname,asset_.description,reportdate,schedstart,daysleft",
                    DefaultSortField = "daysleft",
                    SchemaRef = IncomingPanelSchemaId,
                    Limit = 10,
                    Size = 12
                }
             
            };

            #region CM
            DashboardInitializationService.RegisterWhereClause("workorder", FSDashboardWcBuilder.MaintenanceDashBuild290CMQuery(), "WoMaintenananceBuild290CM", "dashboard:" + BuildPanelAlias290CM);
            DashboardInitializationService.RegisterWhereClause("workorder", "@firstSolarDashboardWcBuilder.MaintenanceCmDashQuery", "WoMaintenananceCmIncoming", "dashboard:" + IncomingPanelCmAlias);
            #endregion

            #region PM
            DashboardInitializationService.RegisterWhereClause("workorder", FSDashboardWcBuilder.MaintenanceDashBuild290PMQuery(), "WoMaintenananceBuild290PM", "dashboard:" + BuildPanelAlias290PM);
            DashboardInitializationService.RegisterWhereClause("workorder", FSDashboardWcBuilder.MaintenanceDashBuild30PQuery(), "WoMaintenananceBuild", "dashboard:" + BuildPanelAlias);
            DashboardInitializationService.RegisterWhereClause("workorder", "@firstSolarDashboardWcBuilder.MaintenanceDashQuery", "WoMaintenananceIncoming", "dashboard:" + IncomingPanelAlias);
            #endregion

            DashboardInitializationService.AddPanelsToDashboard(cmDash, panelsCM);
            DashboardInitializationService.AddPanelsToDashboard(pmDash, panelsPM);

            Log.Info("finishing dashboard registry");
        }

        public int Order => ChartInitializer.ORDER + 1;
    }
}
