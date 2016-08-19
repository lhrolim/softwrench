using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using softwrench.sw4.dashboard.classes.model.entities;

namespace softwrench.sw4.dashboard.classes.startup {
    public class DashboardDefaultDataProvider : ISingletonComponent {
        #region static panels
        private static readonly List<DashboardBasePanel> SRPanelTemplate = new List<DashboardBasePanel>() {
                    new DashboardGraphicPanel() {
                        Alias = "{0}.status.top5",
                        Title = "{0} by Status",
                        Size = 9,
                        Configuration = "application=sr;field=status;type=swRecordCountChart;statusfieldconfig=all;limit=5;showothers=False;options={'series': {'color': '#f65752'}}"
                    },
                    new DashboardGraphicPanel() {
                        Alias = "{0}.status.openclosed.gauge",
                        Title = "Total {0}",
                        Size = 3,
                        Configuration = "application=sr;field=status;type=swCircularGauge;statusfieldconfig=openclosed;limit=0;showothers=False;options={'valueIndicator': {'color': '#e59323', 'type': 'rangeBar'}}"
                    },
                    new DashboardGraphicPanel() {
                        Alias = "{0}.owner.top5",
                        Title = "Top 5 Owners",
                        Size = 6,
                        Configuration = "application=sr;field=owner;type=swRecordCountRotatedChart;statusfieldconfig=all;limit=5;showothers=False;options={'series': {'color': '#808080', 'type': 'line'}}"
                    },
                    new DashboardGraphicPanel() {
                        Alias = "{0}.reportedby.top5",
                        Title = "Top 5 Reporters",
                        Size = 6,
                        Configuration = "application=sr;field=reportedby;type=swRecordCountRotatedChart;statusfieldconfig=all;limit=5;showothers=False;options={'series': {'color': '#4488f2', 'type': 'line'}}"
                    },
                    new DashboardGraphicPanel() {
                        Alias = "{0}.status.openclosed",
                        Title = "Open/Closed {0}",
                        Size = 3,
                        Configuration = "application=sr;field=status;type=swRecordCountPie;statusfieldconfig=openclosed;limit=0;showothers=False;options={'series': {'type': 'doughnut'}, 'swChartsAddons': {'addPiePercentageTooltips': true}}"
                    },
                    new DashboardGridPanel() {
                        Alias = "{0}.grid",
                        Title = "{0}",
                        AppFields = "ticketid,description,reportedby,owner,changedate,status",
                        DefaultSortField = "changedate",
                        SchemaRef = "list",
                        Limit = 15,
                        Size = 12
                    },
            };

        private static readonly List<DashboardBasePanel> WoPanels = new List<DashboardBasePanel>() {
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
                new DashboardGridPanel() {
                    Alias = "wo.grid",
                    Title = "Work Orders",
                    Application = "workorder",
                    AppFields = "wonum,description,location,asset_.description,status",
                    DefaultSortField = "wonum",
                    SchemaRef = "list",
                    Limit = 15,
                    Size = 12
                }
            };

        #endregion

        #region Utils
        private List<DashboardBasePanel> ServiceRequestPanelsInstance(bool? quick = false) {
            var application = "servicerequest";
            var applicationTitle = "Service Requests";
            var entityAlias = "sr";
            if (quick == true) {
                application = "quickservicerequest";
                applicationTitle = "Quick Requests";
                entityAlias = "quicksr";
            }
            // 'cloning' but replacing entity and title
            return SRPanelTemplate
                .Select(p => CopyContents(p, application: application, applicationTitle: applicationTitle, entityAlias: entityAlias))
                .ToList();
        }

        private DashboardBasePanel CopyContents(DashboardBasePanel basePanel, string application = null, string applicationTitle = null, string entityAlias = null) {
            var isGrid = basePanel is DashboardGridPanel;
            DashboardBasePanel panel;
            if (isGrid) {
                panel = new DashboardGridPanel();
                var gridp = (DashboardGridPanel)basePanel;
                var gridPanel = (DashboardGridPanel)panel;
                gridPanel.Application = string.IsNullOrEmpty(application) ? gridp.Application : application;
                gridPanel.AppFields = gridp.AppFields;
                gridPanel.DefaultSortField = gridp.DefaultSortField;
                gridPanel.SchemaRef = gridp.SchemaRef;
                gridPanel.Limit = gridp.Limit;
            } else {
                panel = new DashboardGraphicPanel();
                var graphp = (DashboardGraphicPanel)basePanel;
                var graphPanel = (DashboardGraphicPanel)panel;
                graphPanel.Configuration = string.IsNullOrEmpty(application) ? graphp.Configuration : string.Format("applicationName={0};{1}", application, graphp.Configuration);
            }

            panel.Alias = string.IsNullOrEmpty(entityAlias) ? basePanel.Alias : string.Format(basePanel.Alias, entityAlias);
            panel.Title = string.IsNullOrEmpty(applicationTitle) ? basePanel.Title : string.Format(basePanel.Title, applicationTitle);
            panel.Size = basePanel.Size;

            return panel;
        }

        #endregion

        public List<DashboardBasePanel> ServiceRequestPanels() {
            return ServiceRequestPanelsInstance();
        }

        public List<DashboardBasePanel> QuickServiceRequestPanels() {
            return ServiceRequestPanelsInstance(true);
        }

        public List<DashboardBasePanel> WorkOrderPanels() {
            return WoPanels.Select(p => CopyContents(p)).ToList();
        }

    }
}