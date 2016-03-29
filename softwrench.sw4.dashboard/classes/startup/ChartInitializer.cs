using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using cts.commons.persistence.Util;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Util;
using WebGrease.Css.Extensions;

namespace softwrench.sw4.dashboard.classes.startup {
    public class ChartInitializer : ISWEventListener<ApplicationStartedEvent> {

        private const string WO_CHART_DASHBOARD_TITLE = "[SWWEB-2200](2016-03-28)";
        private const string SR_CHART_DASHBOARD_TITLE = "[SWWEB-2200](2016-03-29)";

        private readonly DisregardingUserSWDBHibernateDaoDecorator _dao = new DisregardingUserSWDBHibernateDaoDecorator(new ApplicationConfigurationAdapter());

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (ShouldExecuteWOChartInitialization()) {
                ExecuteWOChartInitialization();
            }
            if (ShouldExecuteSRChartInitialization()) {
                ExecuteSRChartInitialization();
            }
        }

        #region SR Charts

        private bool ShouldExecuteSRChartInitialization() {
            return !DashBoardExists(SR_CHART_DASHBOARD_TITLE);
        }

        private Dashboard ExecuteSRChartInitialization() {
            var panels = new List<DashboardGraphicPanel>() {
                new DashboardGraphicPanel() {
                    Alias = "sr.status.openclosed.gauge",
                    Title = "Total Service Requests",
                    Size = 3,
                    Configuration = "application=sr;field=status;type=swRecordCountGauge;statusfieldconfig=openclosed;limit=0;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "sr.status.top5",
                    Title = "Service Requests by Status",
                    Size = 9,
                    Configuration = "application=sr;field=status;type=swRecordCountChart;statusfieldconfig=all;limit=5;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "sr.status.line",
                    Title = "Service Requests by Status",
                    Size = 9,
                    Configuration = "application=sr;field=status;type=swRecordCountLineChart;statusfieldconfig=all;limit=0;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "sr.status.openclosed",
                    Title = "Open/Closed Service Requests",
                    Size = 3,
                    Configuration = "application=sr;field=status;type=swRecordCountPie;statusfieldconfig=openclosed;limit=0;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "sr.owner.top5",
                    Title = "Top 5 Owners",
                    Size = 6,
                    Configuration = "application=sr;field=owner;type=swRecordCountRotatedChart;statusfieldconfig=all;limit=5;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "sr.reportedby.top5",
                    Title = "Top 5 Reporters",
                    Size = 6,
                    Configuration = "application=sr;field=reportedby;type=swRecordCountRotatedChart;statusfieldconfig=all;limit=5;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "sr.status.pie",
                    Title = "Service Request Status",
                    Size = 3,
                    Configuration = "application=sr;field=status;type=swRecordCountPie;statusfieldconfig=all;limit=6;showothers=True"
                },
            };

            return CreateDashboard(SR_CHART_DASHBOARD_TITLE, panels);
        }

        #endregion

        #region WO Charts

        private bool ShouldExecuteWOChartInitialization() {
            return !DashBoardExists(WO_CHART_DASHBOARD_TITLE);
        }

        private Dashboard ExecuteWOChartInitialization() {
            var panels = new List<DashboardGraphicPanel>() {
                new DashboardGraphicPanel() {
                    Alias = "wo.status.openclosed.gauge",
                    Title = "Total Work Orders",
                    Size = 3,
                    Configuration = "application=workorder;field=status;type=swRecordCountGauge;statusfieldconfig=openclosed;limit=0;showothers=False"
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
                    Configuration = "application=workorder;field=wopriority;type=swRecordCountLineChart;statusfieldconfig=all;limit=0;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "wo.status.openclosed",
                    Title = "Open/Closed Work Orders",
                    Size = 3,
                    Configuration = "application=workorder;field=status;type=swRecordCountPie;statusfieldconfig=openclosed;limit=0;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "wo.owners.top5",
                    Title = "Top 5 Owners",
                    Size = 6,
                    Configuration = "application=workorder;field=owner;type=swRecordCountRotatedChart;statusfieldconfig=all;limit=5;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "wo.reportedby.top5",
                    Title = "Top 5 Reporters",
                    Size = 6,
                    Configuration = "application=workorder;field=reportedby;type=swRecordCountRotatedChart;statusfieldconfig=all;limit=5;showothers=False"
                },
                 new DashboardGraphicPanel() {
                    Alias = "wo.types",
                    Title = "Work Order Types",
                    Size = 3,
                    Configuration = "application=workorder;field=worktype;type=swRecordCountPie;statusfieldconfig=all;limit=6;showothers=True"
                },
            };

            return CreateDashboard(WO_CHART_DASHBOARD_TITLE, panels);
        }

        #endregion

        #region Utils

        private bool DashBoardExists(string title) {
            var parameters = new ExpandoObject();
            var parameterCollection = (ICollection<KeyValuePair<string, object>>)parameters;
            parameterCollection.Add(new KeyValuePair<string, object>("TITLE", title));

            var count = _dao.CountByNativeQuery("select count(id) from dash_dashboard where title=:TITLE", parameters);
            return count > 0;
        }

        private Dashboard CreateDashboard(string title, ICollection<DashboardGraphicPanel> panels) {
            var now = DateTime.Now;

            // save panels and replace references by hibernate-managed ones
            panels.ForEach(p => {
                p.CreationDate = now;
                p.UpdateDate = now;
                p.Provider = "swChart";
                p.Visible = true;
                p.Filter = new DashboardFilter();
            });
            panels = _dao.BulkSave(panels).ToList();

            // create relationship entities
            var position = 0;
            var panelRelationships = panels
                .Select(p => new DashboardPanelRelationship() {
                    Position = position++,
                    Panel = p
                })
                .ToList();

            // create dashboard
            var dashboard = new Dashboard() {
                Title = title,
                Filter = new DashboardFilter(),
                CreationDate = now,
                UpdateDate = now,
                Panels = panelRelationships
            };

            return _dao.Save(dashboard);
        }

        private class DisregardingUserSWDBHibernateDaoDecorator : SWDBHibernateDAO {
            private readonly SWDBHibernateDAO _dao;

            public DisregardingUserSWDBHibernateDaoDecorator(ApplicationConfigurationAdapter applicationConfiguration) : base(applicationConfiguration, new HibernateUtil(applicationConfiguration)) {
                _dao = new SWDBHibernateDAO(applicationConfiguration, HibernateUtil);
            }

            protected override int? GetCreatedByUser() {
                // force createdby to be 'swadmin' user
                return (int)_dao.FindSingleByNativeQuery<object>("select id from sw_user2 where username = ?", "swadmin");
            }
        }

        #endregion
    }
}
