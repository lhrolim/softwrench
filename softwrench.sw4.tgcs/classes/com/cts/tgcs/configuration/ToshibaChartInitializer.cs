using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.dashboard.classes.startup;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;
using CI = softwrench.sw4.dashboard.classes.startup.ChartInitializer;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.configuration {
    public class ToshibaChartInitializer : ISWEventListener<ApplicationStartedEvent>, IOrdered {

        private readonly DashboardInitializationService _service;
        private readonly DashboardDefaultDataProvider _provider;

        public ToshibaChartInitializer(DashboardInitializationService service, DashboardDefaultDataProvider provider) {
            _service = service;
            _provider = provider;
        }

        public int Order {
            get {
                return CI.ORDER + 1;
            }
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (ApplicationConfiguration.ClientName != "tgcs") {
                return;
            }
            if (MetadataProvider.Application("quickservicerequest", false) == null || _service.DashBoardExists(CI.QuickSrChartDashboardAlias)) {
                return;
            }

            var panels = _provider.QuickServiceRequestPanels();
            _service.CreateDashboard(CI.QuickSrChartDashboardTitle, CI.QuickSrChartDashboardAlias, panels);
        }
    }
}