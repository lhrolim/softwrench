using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using cts.commons.persistence.Util;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;

namespace softwrench.sw4.dashboard.classes.startup {
    public class ChartInitializer : ISWEventListener<ApplicationStartedEvent>, IOrdered {

        private const string WoChartDashboardAlias = "workorder";
        private const string WoChartDashboardTitle = "Work Orders";

        private const string SrChartDashboardAlias = "servicerequest";
        private const string SrChartDashboardTitle = "Service Requests";

        /// <summary>
        /// (WO.status is 'open') != (WO.status isn't 'close') --> special query
        /// </summary>
        private const string WO_STATUS_OPENCLOSED_WHERECLAUSE = @"status = 'CLOSE' or 
                                                                    ((status = 'APPR' or status = 'WPCOND' or status = 'INPRG' or status = 'WORKING' or status = 'WAPPR' or status = 'WMATL') and 
                                                                    (woclass = 'WORKORDER' or woclass = 'ACTIVITY') and historyflag = 0 and istask = 0)";
        /// <summary>
        /// Complete SELECT statistics query for wo.status: includes the statuses's descriptions as labels.
        /// </summary>
        private const string WO_STATUS_WHERECLAUSE_COMPLETE_QUERY =
            @"select COALESCE(CAST(status as varchar), 'NULL') as status, count(*) as countBy, s.description as label 
                from workorder 
                left join synonymdomain s
       	            on status = s.value
  	            where s.domainid = 'WOSTATUS' and s.description is not null
                group by status,s.description
                order by countBy desc";

        /// <summary>
        /// Complete SELECT statistics query for sr.status: includes the statuses's descriptions as labels.
        /// </summary>
        private const string SR_STATUS_WHERECLAUSE_COMPLETE_QUERY = 
            @"select COALESCE(CAST(status as varchar), 'NULL') as status, count(*) as countBy, s.description as label 
                from sr 
                left join synonymdomain s
       	            on status = s.value
  	            where s.domainid = 'SRSTATUS' and s.description is not null
                group by status,s.description
                order by countBy desc";


        private readonly DashboardInitializationService _service;

        public int Order { get { return int.MaxValue - 51; } }

        public ChartInitializer(DashboardInitializationService service) {
            _service = service;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (ShouldExecuteWOChartInitialization()) {
                ExecuteWOChartInitialization();
            }
            if (ShouldExecuteSRChartInitialization()) {
                ExecuteSRChartInitialization();
            }

            _service.RegisterWhereClause("workorder", WO_STATUS_OPENCLOSED_WHERECLAUSE,      "WOOpenCloseDashBoardGauge",  "dashboard:wo.status.openclosed.gauge");
            _service.RegisterWhereClause("workorder", WO_STATUS_OPENCLOSED_WHERECLAUSE,      "WOOpenCloseDashBoardPie",    "dashboard:wo.status.openclosed");
            _service.RegisterWhereClause("workorder", WO_STATUS_WHERECLAUSE_COMPLETE_QUERY,  "WOStatusDashboardQuery",     "dashboard:wo.status.top5");

            _service.RegisterWhereClause("servicerequest", SR_STATUS_WHERECLAUSE_COMPLETE_QUERY, "SRStatusDashboardQuery",     "dashboard:sr.status.top5");
            _service.RegisterWhereClause("servicerequest", SR_STATUS_WHERECLAUSE_COMPLETE_QUERY, "SRStatusDashboardQueryLine", "dashboard:sr.status.line");
            _service.RegisterWhereClause("servicerequest", SR_STATUS_WHERECLAUSE_COMPLETE_QUERY, "SRStatusDashboardQueryPie",  "dashboard:sr.status.pie");
        }

        #region SR Charts

        private bool ShouldExecuteSRChartInitialization() {
            return ApplicationExists(SrChartDashboardAlias) && !_service.DashBoardExists(SrChartDashboardAlias);
        }

        private Dashboard ExecuteSRChartInitialization() {
            var panels = new List<DashboardGraphicPanel>() {
                new DashboardGraphicPanel() {
                    Alias = "sr.status.top5",
                    Title = "Service Requests by Status",
                    Size = 9,
                    Configuration = "application=sr;field=status;type=swRecordCountChart;statusfieldconfig=all;limit=5;showothers=False;options={'series': {'color': '#f65752'}}"
                },
                new DashboardGraphicPanel() {
                    Alias = "sr.status.openclosed.gauge",
                    Title = "Total Service Requests",
                    Size = 3,
                    Configuration = "application=sr;field=status;type=swCircularGauge;statusfieldconfig=openclosed;limit=0;showothers=False;options={'valueIndicator': {'color': '#e59323', 'type': 'rangeBar'}}"
                },
                new DashboardGraphicPanel() {
                    Alias = "sr.owner.top5",
                    Title = "Top 5 Owners",
                    Size = 6,
                    Configuration = "application=sr;field=owner;type=swRecordCountRotatedChart;statusfieldconfig=all;limit=5;showothers=False;options={'series': {'color': '#808080', 'type': 'line'}}"
                },
                new DashboardGraphicPanel() {
                    Alias = "sr.reportedby.top5",
                    Title = "Top 5 Reporters",
                    Size = 6,
                    Configuration = "application=sr;field=reportedby;type=swRecordCountRotatedChart;statusfieldconfig=all;limit=5;showothers=False;options={'series': {'color': '#4488f2', 'type': 'line'}}"
                },
                new DashboardGraphicPanel() {
                    Alias = "sr.status.openclosed",
                    Title = "Open/Closed Service Requests",
                    Size = 3,
                    Configuration = "application=sr;field=status;type=swRecordCountPie;statusfieldconfig=openclosed;limit=0;showothers=False;options={'series': {'type': 'doughnut'}, 'swChartsAddons': {'addPiePercentageTooltips': true}}"
                }
            };

            var grid = new DashboardGridPanel() {
                Alias = "sr.grid",
                Title = "Service Requests",
                Application = "servicerequest",
                AppFields = "ticketid,description,reportedby,owner,changedate,status",
                DefaultSortField = "changedate",
                SchemaRef = "list",
                Limit = 15,
                Size = 12
            };

            return _service.CreateDashboard(SrChartDashboardTitle,SrChartDashboardAlias, panels, grid);
        }

        #endregion

        #region WO Charts

        private bool ShouldExecuteWOChartInitialization() {
            return ApplicationExists(WoChartDashboardAlias) && !_service.DashBoardExists(WoChartDashboardAlias);
        }

        private Dashboard ExecuteWOChartInitialization() {
            var panels = new List<DashboardGraphicPanel>() {
                new DashboardGraphicPanel() {
                    Alias = "wo.status.openclosed.gauge",
                    Title = "Total Work Orders",
                    Size = 3,
                    Configuration = "application=workorder;field=status;type=swCircularGauge;statusfieldconfig=openclosed;limit=0;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "wo.status.top5",
                    Title = "Work Order by Status",
                    Size = 9,
                    Configuration = "application=workorder;field=status;type=swRecordCountChart;statusfieldconfig=all;limit=5;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "wo.priority",
                    Title = "Work Order by Priority",
                    Size = 9,
                    Configuration = "application=workorder;field=wopriority;type=swRecordCountLineChart;statusfieldconfig=all;limit=0;showothers=False;options={'series': {'color': '#808080', 'label': {'visible': false}, 'type': 'line'}}"
                },
                new DashboardGraphicPanel() {
                    Alias = "wo.status.openclosed",
                    Title = "Open/Closed Work Orders",
                    Size = 3,
                     Configuration = "application=workorder;field=status;type=swRecordCountPie;statusfieldconfig=openclosed;limit=0;showothers=False;options={'legend': {'visible': false}, 'swChartsAddons': {'addPieLabelAndPercentageLabels': true}}"
                },
                new DashboardGraphicPanel() {
                    Alias = "wo.owners.top5",
                    Title = "Top 5 Owners",
                    Size = 6,
                    Configuration = "application=workorder;field=owner;type=swRecordCountRotatedChart;statusfieldconfig=all;limit=5;showothers=False;options={'series': {'color': '#e59323'}}"
                },
                new DashboardGraphicPanel() {
                    Alias = "wo.reportedby.top5",
                    Title = "Top 5 Reporters",
                    Size = 6,
                    Configuration = "application=workorder;field=reportedby;type=swRecordCountRotatedChart;statusfieldconfig=all;limit=5;showothers=False;options={'series': {'color': '#39b54a'}}"
                },
                 new DashboardGraphicPanel() {
                    Alias = "wo.types",
                    Title = "Work Order Types",
                    Size = 3,
                    Configuration = "application=workorder;field=worktype;type=swRecordCountPie;statusfieldconfig=all;limit=6;showothers=True;options={'series': {'type': 'doughnut'}, 'swChartsAddons': {'addPiePercentageTooltips': true}, 'tooltip': {'enabled': false}}"
                },
            };

            var grid = new DashboardGridPanel() {
                Alias = "wo.grid",
                Title = "Work Orders",
                Application = "workorder",
                AppFields = "wonum,description,location,asset_.description,status",
                DefaultSortField = "wonum",
                SchemaRef = "list",
                Limit = 15,
                Size = 12
            };

            return _service.CreateDashboard(WoChartDashboardTitle, WoChartDashboardAlias, panels, grid);
        }

        #endregion

        #region Utils

        protected bool ApplicationExists(string application) {
            return MetadataProvider.Application(application, false) != null;
        }

        #endregion
    }
}
