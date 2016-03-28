using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using cts.commons.persistence.Util;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Util;

namespace softwrench.sw4.dashboard.classes.startup {
    public class ChartInitializer : ISWEventListener<ApplicationStartedEvent> {

        private const string CHART_DASHBOARD_TITLE = "[SWWEB-2200](2016-03-28)";

        private readonly DisregardingUserSWDBHibernateDaoDecorator _dao = new DisregardingUserSWDBHibernateDaoDecorator(new ApplicationConfigurationAdapter());

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (!ShouldExecuteInitialization()) return;

            var now = DateTime.Now;

            var panels = new List<DashboardGraphicPanel>() {
                new DashboardGraphicPanel() {
                    Alias = "wo.status.openclosed.gauge",
                    Title = "Total Work Orders",
                    CreationDate = now,
                    UpdateDate = now,
                    Visible = true,
                    Filter = new DashboardFilter(),
                    Size = 4,
                    Provider = "swChart",
                    Configuration = "application=workorder;field=status;type=swRecordCountGauge;statusfieldconfig=openclosed;limit=0;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "wo.status.top5",
                    Title = "Work Order by Status",
                    CreationDate = now,
                    UpdateDate = now,
                    Visible = true,
                    Filter = new DashboardFilter(),
                    Size = 8,
                    Provider = "swChart",
                    Configuration = "application=workorder;field=status;type=swRecordCountChart;statusfieldconfig=all;limit=5;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "wo.priority",
                    Title = "Work Order by Priority",
                    CreationDate = now,
                    UpdateDate = now,
                    Visible = true,
                    Filter = new DashboardFilter(),
                    Size = 8,
                    Provider = "swChart",
                    Configuration = "application=workorder;field=wopriority;type=swRecordCountLineChart;statusfieldconfig=all;limit=0;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "wo.status.openclosed",
                    Title = "Open/Closed Work Orders",
                    CreationDate = now,
                    UpdateDate = now,
                    Visible = true,
                    Filter = new DashboardFilter(),
                    Size = 4,
                    Provider = "swChart",
                    Configuration = "application=workorder;field=status;type=swRecordCountPie;statusfieldconfig=openclosed;limit=0;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "wo.owners.top5",
                    Title = "Top 5 Owners",
                    CreationDate = now,
                    UpdateDate = now,
                    Visible = true,
                    Filter = new DashboardFilter(),
                    Size = 6,
                    Provider = "swChart",
                    Configuration = "application=workorder;field=owner;type=swRecordCountRotatedChart;statusfieldconfig=all;limit=5;showothers=False"
                },
                new DashboardGraphicPanel() {
                    Alias = "wo.reportedby.top5",
                    Title = "Top 5 Reporters",
                    CreationDate = now,
                    UpdateDate = now,
                    Visible = true,
                    Filter = new DashboardFilter(),
                    Size = 6,
                    Provider = "swChart",
                    Configuration = "application=workorder;field=reportedby;type=swRecordCountRotatedChart;statusfieldconfig=all;limit=5;showothers=False"
                },
                 new DashboardGraphicPanel() {
                    Alias = "wo.types",
                    Title = "Work Order Types",
                    CreationDate = now,
                    UpdateDate = now,
                    Visible = true,
                    Filter = new DashboardFilter(),
                    Size = 4,
                    Provider = "swChart",
                    Configuration = "application=workorder;field=worktype;type=swRecordCountPie;statusfieldconfig=all;limit=6;showothers=True"
                },
            };

            // save panels and replace references by hibernate-managed ones
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
                Title = CHART_DASHBOARD_TITLE,
                Filter = new DashboardFilter(),
                CreationDate = now,
                UpdateDate = now,
                Panels = panelRelationships
            };

            _dao.Save(dashboard);
        }

        private bool ShouldExecuteInitialization() {
            var parameters = new ExpandoObject();
            var parameterCollection = (ICollection<KeyValuePair<string, object>>)parameters;
            parameterCollection.Add(new KeyValuePair<string, object>("TITLE", CHART_DASHBOARD_TITLE));

            var count = _dao.CountByNativeQuery("select count(id) from dash_dashboard where title=:TITLE", parameters);
            return count <= 0;
        }

        private class DisregardingUserSWDBHibernateDaoDecorator : SWDBHibernateDAO {
            private readonly SWDBHibernateDAO _dao;

            public DisregardingUserSWDBHibernateDaoDecorator(ApplicationConfigurationAdapter applicationConfiguration) : base(applicationConfiguration, new HibernateUtil(applicationConfiguration)) {
                _dao = new SWDBHibernateDAO(applicationConfiguration, HibernateUtil);
            }

            protected override int? GetCreatedByUser() {
                // force createdby to be 'swadmin' user
                return (int) _dao.FindSingleByNativeQuery<object>("select id from sw_user2 where username = ?", "swadmin");
            }
        }
    }
}
