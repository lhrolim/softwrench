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

        public const string MaintenanceDashAlias = "fs.maintenance";

        public const string IncomingPanelAlias = "fs.maintenance.incoming";
        public const string IncomingPanelSchemaId = "workpackageincomingdash";

        public const string BuildPanelAlias = "fs.maintenance.build";
        public const string BuildPanelAlias290 = "fs.maintenance.290";
        public const string BuildPanelSchemaId = "workpackagebuilddash";
        public const string BuildPanel290SchemaId = "workpackagebuild290dash";

        private ILog Log = LogManager.GetLogger(typeof(FirstSolarDashboardInitializer));

        [Import]
        public DashboardInitializationService DashboardInitializationService { get; set; }

        [Import]
        public IWhereClauseFacade WhereClauseFacade { get; set; }

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Transactional(DBType.Swdb)]
        public virtual void HandleEvent(ApplicationStartedEvent eventToDispatch)
        {

            Log.Info("initing fs dashboard registry");

            var maintenanceDash = DashboardInitializationService.FindByAlias(MaintenanceDashAlias) ??
                                  DashboardInitializationService.CreateDashboard("Maintenance Queues", MaintenanceDashAlias, new List<DashboardBasePanel>());
            maintenanceDash.Application = "_WorkPackage";
            Dao.Save(maintenanceDash);

            var panels = new List<DashboardBasePanel> {
                new DashboardGridPanel() {
                    Alias = BuildPanelAlias290,
                    Title = "29-0 Days Queue",
                    Application = "workorder",
                    AppFields = "wonum,#wpnum,plannerscheduler_.displayname,location_.facilityname,assetnum,schedstart,#buildcomplete",
                    DefaultSortField = "schedstart",
                    SchemaRef = BuildPanel290SchemaId,
                    Limit = 10,
                    Size = 12
                },
                new DashboardGridPanel() {
                    Alias = BuildPanelAlias,
                    Title = "Build Queue (54-30 Days)",
                    Application = "workorder",
                    AppFields = "wonum,#wpnum,plannerscheduler_.displayname,location_.facilityname,assetnum,schedstart,#buildcomplete",
                    DefaultSortField = "schedstart",
                    SchemaRef = BuildPanelSchemaId,
                    Limit = 10,
                    Size = 12
                },
                new DashboardGridPanel() {
                    Alias = IncomingPanelAlias,
                    Title = "Incoming Queue (55+ Days)",
                    Application = "workorder",
                    AppFields = "wonum,plannerscheduler_.displayname,location_.facilityname,assetnum,schedstart",
                    DefaultSortField = "schedstart",
                    SchemaRef = IncomingPanelSchemaId,
                    Limit = 10,
                    Size = 12
                }
            };
            DashboardInitializationService.RegisterWhereClause("workorder", "@firstSolarDashboardWcBuilder.MaintenanceDashQuery", "WoMaintenananceIncoming", "dashboard:" + IncomingPanelAlias);
            DashboardInitializationService.RegisterWhereClause("workorder", "@firstSolarDashboardWcBuilder.MaintenanceDashQuery", "WoMaintenananceBuild", "dashboard:" + BuildPanelAlias);
            DashboardInitializationService.RegisterWhereClause("workorder", "@firstSolarDashboardWcBuilder.MaintenanceDashQuery", "WoMaintenananceBuild290", "dashboard:" + BuildPanelAlias290);
            DashboardInitializationService.AddPanelsToDashboard(maintenanceDash, panels);

            Log.Info("finishing dashboard registry");
        }

        public int Order => ChartInitializer.ORDER + 1;
    }
}
