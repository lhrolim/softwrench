﻿using System.Collections.Generic;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;
using log4net;
using softwrench.sw4.dashboard.classes.model;
using softwrench.sw4.dashboard.classes.model.entities;
using softwrench.sw4.dashboard.classes.startup;
using softWrench.sW4.Util;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.configuration.Dashboard {
    public class ChicagoChartInitializer : ISWEventListener<ApplicationStartedEvent>, IOrdered {

        protected static readonly ILog Log = LogManager.GetLogger(typeof(ChicagoChartInitializer));

        private const string ChicagoSRDepartment = "chicago.sr.department";
        private const string ChicagoSRTickettype = "chicago.sr.tickettype";
        private const string ChicagoSRDailyTickets = "chicago.sr.dailytickets";

        private readonly DashboardInitializationService _service;



        public ChicagoChartInitializer(DashboardInitializationService service) {
            _service = service;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (ApplicationConfiguration.ClientName != "chicago") {
                return;
            }

            var srDashboard = _service.FindByAlias(ChartInitializer.SrChartDashboardAlias);
            StatisticsQueryProvider.AddCustomSelectQuery("dashboard:chicago.sr.dailytickets", ChicagoDashBoardWhereClausedProvider.SRStatusDaily);
            _service.RegisterWhereClause("servicerequest", "@chicagoDashBoardWhereClausedProvider.GetTicketCountQuery", "SROpenedDaily", "dashboard:chicago.sr.dailytickets");

            if (srDashboard == null) {
                Log.Warn(string.Format("Could not add chicago's SR charts because there is no dashboard with alias '{0}' registered", ChartInitializer.SrChartDashboardAlias));
                return;
            }

            var panels = new List<DashboardBasePanel> {
                new DashboardGraphicPanel {
                    Alias = ChicagoSRDepartment,
                    Title = "Service Requests by Department",
                    Size = 12,
                    Configuration = "application=sr;field=department;type=swRecordCountChart;limit=0;showothers=False"
                },
                new DashboardGraphicPanel {
                    Alias = ChicagoSRTickettype,
                    Title = "Service Requests by Ticket Type",
                    Size = 12,
                    Configuration = "application=sr;field=tickettype;type=swRecordCountChart;limit=0;showothers=False"
                },
                  new DashboardGraphicPanel() {
                    Alias = ChicagoSRDailyTickets,
                    Title = "Number of Opened Service Requests for current month",
                    Size = 12,
                    Configuration = "application=sr;field=day;type=swRecordCountChart;limit=0;showothers=False;keepServerSort=true;clickDtoProvider=chicagoChartService.dailyopenedTicketsClicked;options={'argumentAxis':{'title':{'text': 'Days' }}}"
                },
            };
            _service.AddPanelsToDashboard(srDashboard, panels);
        }

        public int Order {
            get {
                return ChartInitializer.ORDER + 1;
            }
        }
    }
}