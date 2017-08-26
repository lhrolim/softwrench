using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.dashboard.classes.controller;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dashboard {
    [OverridingComponent(ClientFilters = "firstsolar")]
    public class FirstSolarUserDashboardManager : UserDashboardManager {
        private readonly SWDBHibernateDAO _dao;

        public FirstSolarUserDashboardManager(SWDBHibernateDAO dao) : base(dao) {
            _dao = dao;
        }

        protected override void FilterPanelBasedOnRole(InMemoryUser user, DashboardBasePanel panel) {
            base.FilterPanelBasedOnRole(user, panel);
            if (!(panel is DashboardGridPanel)) {
                return;
            }
            var gridPanel = (DashboardGridPanel)panel;
            if (ShouldSeeMaintenanceDash(user, gridPanel)) {
                gridPanel.Visible = true;
            } else if (ShouldNotSeeMaintenanceDash(user, gridPanel)) {
                gridPanel.Visible = false;
            }
        }

        protected virtual bool ShouldSeeMaintenanceDash(InMemoryUser user, DashboardGridPanel gridPanel) {
            var baseCondition = !user.IsSwAdmin() && "workorder".Equals(gridPanel.Application) && !gridPanel.Visible && IsMaintenanceDash(gridPanel);
            if (!baseCondition) {
                return false;
            }

            var permissions = user.MergedUserProfile.Permissions;
            var wpPermission = permissions.FirstOrDefault(p => p.ApplicationName.Equals("_WorkPackage"));
            return wpPermission != null && !wpPermission.HasNoPermissions;
        }

        protected virtual bool ShouldNotSeeMaintenanceDash(InMemoryUser user, DashboardGridPanel gridPanel) {
            var baseCondition = !user.IsSwAdmin() && "workorder".Equals(gridPanel.Application) && gridPanel.Visible && IsMaintenanceDash(gridPanel);
            if (!baseCondition) {
                return false;
            }

            var permissions = user.MergedUserProfile.Permissions;
            var wpPermission = permissions.FirstOrDefault(p => p.ApplicationName.Equals("_WorkPackage"));
            return wpPermission == null || wpPermission.HasNoPermissions;
        }

        protected virtual bool IsMaintenanceDash(DashboardBasePanel panel) {
            return panel.Alias.EqualsAny(new List<string>()
            {
                FirstSolarDashboardInitializer.IncomingPanelAlias,
                FirstSolarDashboardInitializer.BuildPanelAlias,
                FirstSolarDashboardInitializer.BuildPanelAlias290PM,
                FirstSolarDashboardInitializer.BuildPanelAlias290CM,
                FirstSolarDashboardInitializer.IncomingPanelCmAlias
            });
        }
    }
}
