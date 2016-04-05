﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using cts.commons.persistence.Util;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;
using WebGrease.Css.Extensions;

namespace softwrench.sw4.dashboard.classes.startup {
    public class ChartInitializer : ISWEventListener<ApplicationStartedEvent>, IOrdered {

        private const string WoChartDashboardAlias = "workorder";
        private const string SrChartDashboardAlias = "servicerequest";
        private const string SrChartDashboardTitle = "Service Requests";
        private const string WoChartDashboardTitle = "Work Orders";

        /// <summary>
        /// (WO.status is 'open') != (WO.status isn't 'close') --> special query
        /// </summary>
        private const string WO_STATUS_OPENCLOSED_WHERECLAUSE = @"status = 'CLOSE' or 
                                                                    ((status = 'APPR' or status = 'WPCOND' or status = 'INPRG' or status = 'WORKING' or status = 'WAPPR' or status = 'WMATL') and 
                                                                    (woclass = 'WORKORDER' or woclass = 'ACTIVITY') and historyflag = 0 and istask = 0)";

        private readonly DisregardingUserSWDBHibernateDaoDecorator _dao = new DisregardingUserSWDBHibernateDaoDecorator(new ApplicationConfigurationAdapter());
        private readonly IWhereClauseFacade _whereClauseFacade;

        public int Order { get { return int.MaxValue - 51; } }

        public ChartInitializer(IWhereClauseFacade whereClauseFacade) {
            _whereClauseFacade = whereClauseFacade;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (ShouldExecuteWOChartInitialization()) {
                ExecuteWOChartInitialization();
            }
            if (ShouldExecuteSRChartInitialization()) {
                ExecuteSRChartInitialization();
            }

            var openClosedGauge = "dashboard:wo.status.openclosed.gauge";
            RegisterWhereClause("workorder", WO_STATUS_OPENCLOSED_WHERECLAUSE, "OpenCloseDashBoardPie", openClosedGauge);

            var openClosedPie = "dashboard:wo.status.openclosed";
            RegisterWhereClause("workorder", WO_STATUS_OPENCLOSED_WHERECLAUSE, "OpenCloseDashBoardPie", openClosedPie);
        }

        #region SR Charts

        private bool ShouldExecuteSRChartInitialization() {
            return !DashBoardExists(SrChartDashboardAlias);
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

            return CreateDashboard(SrChartDashboardTitle,SrChartDashboardAlias, panels);
        }

        #endregion

        #region WO Charts

        private bool ShouldExecuteWOChartInitialization() {
            return !DashBoardExists(WoChartDashboardAlias);
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

            return CreateDashboard(WoChartDashboardTitle,WoChartDashboardAlias, panels);
        }

        #endregion

        #region Utils

        private bool DashBoardExists(string alias) {
            var parameters = new ExpandoObject();
            var parameterCollection = (ICollection<KeyValuePair<string, object>>)parameters;
            parameterCollection.Add(new KeyValuePair<string, object>("alias", alias));

            var count = _dao.CountByNativeQuery("select count(id) from dash_dashboard where alias=:alias", parameters);
            return count > 0;
        }

        private Dashboard CreateDashboard(string title,string alias, ICollection<DashboardGraphicPanel> panels) {
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
                Filter = new DashboardFilter(),
                CreationDate = now,
                UpdateDate = now,
                Alias = alias,
                System = true,
                Application = alias,
                Title = title,
                Panels = panelRelationships,
                Active = true
            };

            return _dao.Save(dashboard);
        }


        private void RegisterWhereClause(string application, string query,string alias, string metadataId) {
            _whereClauseFacade.Register(application, query, new WhereClauseRegisterCondition() {
                Alias = alias,
                AppContext = new ApplicationLookupContext() {
                    MetadataId = metadataId,
                }
            });
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