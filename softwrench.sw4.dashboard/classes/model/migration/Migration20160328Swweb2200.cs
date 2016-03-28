using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence.Util;
using FluentMigrator;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Util;

namespace softwrench.sw4.dashboard.classes.model.migration {

    [Migration(201603281000)]
    public class Migration20160328Swweb2200 : Migration {

        public override void Up() {
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

            var dao = new DisregardingUserSWDBHibernateDaoDecorator(new ApplicationConfigurationAdapter());

            // save panels and replace references by hibernate-managed references
            panels = dao.BulkSave(panels).ToList();

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
                Title = "SWWEB2200",
                Filter = new DashboardFilter(),
                CreationDate = now,
                UpdateDate = now,
                Panels = panelRelationships
            };

            dao.Save(dashboard);
        }

        public override void Down() {
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
    }
}
