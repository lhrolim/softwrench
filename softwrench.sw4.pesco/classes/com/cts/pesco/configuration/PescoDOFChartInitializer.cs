using System.Collections.Generic;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.dashboard.classes.model.entities;
using softwrench.sw4.dashboard.classes.startup;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Util;

namespace softwrench.sw4.pesco.classes.com.cts.pesco.configuration {
    public class PescoDOFChartInitializer : ISWEventListener<ApplicationStartedEvent>, IOrdered {

        private const string PESCO_DOF_DASHBOARD_ALIAS = "pesco.dof";

        private readonly DashboardInitializationService _service;

        public PescoDOFChartInitializer(DashboardInitializationService service) {
            _service = service;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (!"pesco".Equals(ApplicationConfiguration.ClientName)) {
                return;
            }

            if (_service.DashBoardExists(PESCO_DOF_DASHBOARD_ALIAS)) return;
            var panels = new List<DashboardGraphicPanel> {
                new DashboardGraphicPanel() {
                    Alias = "pesco.dof.device_value",
                    Title = "Device Values: Dummy",
                    Size = 12,
                    Configuration = "application=pesco_device_value;type=swRecordCountChart;action=device_value"
                }
            };
            _service.CreateDashboard("DOF", PESCO_DOF_DASHBOARD_ALIAS, panels);
        }


        public int Order { get { return int.MaxValue - 50; } }
    }
}
