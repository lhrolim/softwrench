using System.Collections.Generic;
using System.Collections.ObjectModel;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Metadata;

namespace softwrench.sw4.dashboard.classes.startup {
    public class ChartInitializer : ISWEventListener<ApplicationStartedEvent>, IOrdered {

        public const int ORDER = int.MaxValue - 51;

        public const string WoChartDashboardAlias = "workorder";
        public const string WoChartDashboardTitle = "Work Orders";

        public const string SrChartDashboardAlias = "servicerequest";
        public const string SrChartDashboardTitle = "Service Requests";

        public const string QuickSrChartDashboardAlias = "quickservicerequest";
        public const string QuickSrChartDashboardTitle = "Quick Requests";

        public static readonly Dictionary<string, string> AliasMetadataIdDict = new Dictionary<string, string>() {
            { "dashboard:wo.status.openclosed.gauge", "WOOpenCloseDashBoardGauge" },
            { "dashboard:wo.status.openclosed", "WOOpenCloseDashBoardPie" },
            { "dashboard:wo.status.top5", "WOStatusDashboardQuery" },
            { "dashboard:sr.status.top5", "SRStatusDashboardQuery" },
            { "dashboard:sr.status.line", "SRStatusDashboardQueryLine" },
            { "dashboard:sr.status.pie", "SRStatusDashboardQueryPie" },
            { "dashboard:quicksr.status.top5", "QuickSRStatusDashboardQuery" },
            { "dashboard:quicksr.status.line", "QuickSRStatusDashboardQueryLine" },
            { "dashboard:quicksr.status.pie", "QuickSRStatusDashboardQueryPie" }
        };

        /// <summary>
        /// (WO.status is 'open') != (WO.status isn't 'close') --> special query
        /// </summary>
        private const string WO_STATUS_OPENCLOSED_WHERECLAUSE = @"status = 'CLOSE' or 
                                                                    ((status = 'APPR' or status = 'WPCOND' or status = 'INPRG' or status = 'WORKING' or status = 'WAPPR' or status = 'WMATL') and 
                                                                    (woclass = 'WORKORDER' or woclass = 'ACTIVITY') and historyflag = 0 and istask = 0)";
    
        private readonly DashboardInitializationService _service;
        private readonly DashboardDefaultDataProvider _provider;

        public int Order {
            get { return ORDER; }
        }

        public ChartInitializer(DashboardInitializationService service, DashboardDefaultDataProvider provider) {
            _service = service;
            _provider = provider;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (ShouldExecuteWOChartInitialization()) {
                ExecuteWOChartInitialization();
            }
            if (ShouldExecuteSRChartInitialization()) {
                ExecuteSRChartInitialization();
            }

            _service.RegisterWhereClause("workorder", WO_STATUS_OPENCLOSED_WHERECLAUSE, "WOOpenCloseDashBoardGauge", "dashboard:wo.status.openclosed.gauge");
            _service.RegisterWhereClause("workorder", WO_STATUS_OPENCLOSED_WHERECLAUSE, "WOOpenCloseDashBoardPie", "dashboard:wo.status.openclosed");
            _service.RegisterWhereClause("workorder", "s.domainid = 'WOSTATUS' and s.description is not null", "WOStatusDashboardQuery", "dashboard:wo.status.top5");

            _service.RegisterWhereClause("servicerequest", "s.domainid = 'SRSTATUS' and s.description is not null", "SRStatusDashboardQuery", "dashboard:sr.status.top5");
            _service.RegisterWhereClause("servicerequest", "s.domainid = 'SRSTATUS' and s.description is not null", "SRStatusDashboardQueryLine", "dashboard:sr.status.line");
            _service.RegisterWhereClause("servicerequest", "s.domainid = 'SRSTATUS' and s.description is not null", "SRStatusDashboardQueryPie", "dashboard:sr.status.pie");

            _service.RegisterWhereClause("quickservicerequest", "s.domainid = 'SRSTATUS' and s.description is not null", "QuickSRStatusDashboardQuery", "dashboard:quicksr.status.top5");
            _service.RegisterWhereClause("quickservicerequest", "s.domainid = 'SRSTATUS' and s.description is not null", "QuickSRStatusDashboardQueryLine", "dashboard:quicksr.status.line");
            _service.RegisterWhereClause("quickservicerequest", "s.domainid = 'SRSTATUS' and s.description is not null", "QuickSRStatusDashboardQueryPie", "dashboard:quicksr.status.pie");

        }

        #region SR Charts

        private bool ShouldExecuteSRChartInitialization() {
            return ApplicationExists(SrChartDashboardAlias) && !_service.DashBoardExists(SrChartDashboardAlias);
        }

        private Dashboard ExecuteSRChartInitialization() {
            var panels = _provider.ServiceRequestPanels();
            return _service.CreateDashboard(SrChartDashboardTitle, SrChartDashboardAlias, panels);
        }

        #endregion

        #region WO Charts

        private bool ShouldExecuteWOChartInitialization() {
            return ApplicationExists(WoChartDashboardAlias) && !_service.DashBoardExists(WoChartDashboardAlias);
        }

        private Dashboard ExecuteWOChartInitialization() {
            var panels = _provider.WorkOrderPanels();
            return _service.CreateDashboard(WoChartDashboardTitle, WoChartDashboardAlias, panels);
        }

        #endregion

        #region Utils

        protected bool ApplicationExists(string application) {
            return MetadataProvider.Application(application, false) != null;
        }

        #endregion
    }
}
