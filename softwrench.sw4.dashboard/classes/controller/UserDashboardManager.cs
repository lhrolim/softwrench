﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.simpleinjector;
using log4net;
using log4net.Core;
using softwrench.sw4.api.classes.user;
using softwrench.sw4.dashboard.classes.model;
using softwrench.sw4.dashboard.classes.model.entities;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.dashboard.classes.controller {
    public class UserDashboardManager : ISingletonComponent {

        private readonly SWDBHibernateDAO _dao;

        private static ILog Log = LogManager.GetLogger(typeof(UserDashboardManager));

        public UserDashboardManager(SWDBHibernateDAO dao) {
            _dao = dao;
            Log.Debug("init log");
        }


        public IEnumerable<Dashboard> LoadUserDashboars(InMemoryUser currentUser) {
            var profiles = currentUser.ProfileIds;
            var profilesArray = profiles as int?[] ?? profiles.ToArray();
            var applications = currentUser.MergedUserProfile.Permissions.Where(p => !p.HasNoPermissions).Select(p => p.ApplicationName).ToArray();

            IEnumerable<Dashboard> loadedUserDashboars;
            if (currentUser.IsSwAdmin()) {
                loadedUserDashboars = _dao.FindByQuery<Dashboard>(Dashboard.SwAdminQuery);

            } else if (profilesArray.Any() && !currentUser.IsSwAdmin()) {
                //sw admin can always see everything
                var statement = Dashboard.ByUserAndApplications(currentUser.ProfileIds);
                loadedUserDashboars = _dao.FindByQuery<Dashboard>(statement, currentUser.UserId, applications);
            } else {
                loadedUserDashboars = _dao.FindByQuery<Dashboard>(Dashboard.ByUserAndApplicationsNoProfile, currentUser.UserId, applications);
            }

            return FilterViewableWidgetsByUser(loadedUserDashboars, currentUser);
        }

        private IEnumerable<Dashboard> FilterViewableWidgetsByUser(IEnumerable<Dashboard> loadedUserDashboards, ISWUser currentUser) {
            var filterViewableWidgetsByUser = loadedUserDashboards as Dashboard[] ?? loadedUserDashboards.ToArray();
            foreach (var dashboard in filterViewableWidgetsByUser) {
                foreach (var panelRelationship in dashboard.PanelsSet) {
                    FilterPanelBasedOnRole(currentUser, panelRelationship.Panel);
                }
            }
            return filterViewableWidgetsByUser;
        }

        private static void FilterPanelBasedOnRole(ISWUser currentUser, DashboardBasePanel panel) {
            if (!(panel is DashboardGridPanel)) {
                return;
            }

            var gridPanel = (DashboardGridPanel)panel;
            var overridenRole = MetadataProvider.ApplicationRoleAlias.ContainsKey(gridPanel.Application)
                ? MetadataProvider.ApplicationRoleAlias[gridPanel.Application]
                : null;
            if (!currentUser.IsInRole(gridPanel.Application) && (overridenRole != null && !currentUser.IsInRole(overridenRole))) {
                Log.DebugFormat("making panel {0} invisible due to abscence of role for application {1}", gridPanel.Alias,
                    gridPanel.Application);
                gridPanel.Visible = false;
            }
        }

        public IEnumerable<DashboardBasePanel> LoadUserPanels(ISWUser currentUser, string panelType) {
            var profiles = currentUser.ProfileIds;
            var enumerable = profiles as int?[] ?? profiles.ToArray();
            string entityName;
            switch (panelType) {
                case DashboardConstants.PanelTypes.Grid:
                    entityName = typeof(DashboardGridPanel).Name;
                    break;

                case DashboardConstants.PanelTypes.Graphic:
                    entityName = typeof (DashboardGraphicPanel).Name;
                    break;

                default:
                    throw new NotSupportedException("graphics are not yet implemented");
            }
            IEnumerable<DashboardBasePanel> result;
            if (currentUser.IsSwAdmin()) {
                result = _dao.FindByQuery<DashboardBasePanel>(DashboardBasePanel.SwAdminQuery(entityName));
            } else if (enumerable.Any()) {
                var dashboardBasePanels = _dao.FindByQuery<DashboardBasePanel>(DashboardBasePanel.ByUser(entityName, enumerable), currentUser.UserId);
                result = dashboardBasePanels;
            } else {
                result = _dao.FindByQuery<DashboardBasePanel>(DashboardBasePanel.ByUserNoProfile(entityName), currentUser.UserId);
            }
            foreach (var panel in result) {
                FilterPanelBasedOnRole(currentUser, panel);
            }
            return result.Where(f => f.Visible);
        }
    }
}
