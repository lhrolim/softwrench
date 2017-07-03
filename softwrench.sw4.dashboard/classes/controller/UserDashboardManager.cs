using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using log4net;
using softwrench.sw4.api.classes.user;
using softwrench.sw4.dashboard.classes.model;
using softwrench.sw4.dashboard.classes.model.entities;
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
            if (!applications.Any()) {
                // HQL requires a non-empty list for IN parameters
                applications = new string[] { null };
            }

            IEnumerable<Dashboard> loadedUserDashboars;
            if (currentUser.IsSwAdmin()) {
                // sw admin can always see everything
                loadedUserDashboars = _dao.FindByQuery<Dashboard>(Dashboard.SwAdminQuery);

            } else if (profilesArray.Any() && !currentUser.IsSwAdmin()) {

                var statement = Dashboard.ByUserAndApplications(currentUser.ProfileIds);
                loadedUserDashboars = _dao.FindByQuery<Dashboard>(statement, currentUser.UserId, applications);

            } else {
                loadedUserDashboars = _dao.FindByQuery<Dashboard>(Dashboard.ByUserAndApplicationsNoProfile, currentUser.UserId, applications);
            }

            return FilterViewableWidgetsByUser(loadedUserDashboars, currentUser);
        }

        private IEnumerable<Dashboard> FilterViewableWidgetsByUser(IEnumerable<Dashboard> loadedUserDashboards, InMemoryUser currentUser) {
            var filterViewableWidgetsByUser = loadedUserDashboards as Dashboard[] ?? loadedUserDashboards.ToArray();
            foreach (var dashboard in filterViewableWidgetsByUser) {
                foreach (var panelRelationship in dashboard.PanelsSet) {
                    FilterPanelBasedOnRole(currentUser, panelRelationship.Panel);
                }
            }
            return filterViewableWidgetsByUser;
        }

        protected virtual void FilterPanelBasedOnRole(InMemoryUser user, DashboardBasePanel panel) {
            if (!(panel is DashboardGridPanel)) {
                return;
            }
            var gridPanel = (DashboardGridPanel)panel;

            var appPermission = user.MergedUserProfile.Permissions.FirstOrDefault(p => p.ApplicationName == gridPanel.Application);

            if (!user.IsSwAdmin() && (appPermission == null || appPermission.HasNoPermissions)) {
                Log.DebugFormat("making panel {0} invisible due to abscence of role for application {1}", gridPanel.Alias, gridPanel.Application);
                gridPanel.Visible = false;
            }
        }

        public IEnumerable<DashboardBasePanel> LoadUserPanels(InMemoryUser currentUser, string panelType) {
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
