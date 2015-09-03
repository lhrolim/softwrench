using System;
using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.api.classes;
using softwrench.sw4.api.classes.user;
using softwrench.sw4.dashboard.classes.model;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softWrench.sW4.Metadata.Properties;
using softWrench.sW4.Util;

namespace softwrench.sw4.dashboard.classes.controller {
    public class DashboardMenuManager : IMenuManager {

        private readonly UserDashboardManager _userDashboardManager;

        public DashboardMenuManager(UserDashboardManager userDashboardManager) {
            _userDashboardManager = userDashboardManager;
        }

        public MenuDefinition ModifyMenu(MenuDefinition securedMenu, ISWUser user) {
            //if (!ApplicationConfiguration.IsLocal()) {
            //    //TODO: remove this when it´s good for qa
            //    return securedMenu;
            //}

            // If the dashboards are not enabled, do not load the dashboards.
            if ("false".Equals(MetadataProvider.GlobalProperty("dashboard.enabled"))) {
                return securedMenu;
            }

            var leafs = new List<MenuBaseDefinition>();

            if (!user.Genericproperties.ContainsKey(DashboardConstants.DashBoardsProperty)) {
                //caching
                user.Genericproperties[DashboardConstants.DashBoardsProperty] = _userDashboardManager.LoadUserDashboars(user);
            }

            var dashboards = user.Genericproperties[DashboardConstants.DashBoardsProperty] as IEnumerable<Dashboard>;


            MenuBaseDefinition dashBoardMenu;

            var canCreateDashBoards = CanCreateDashBoards(user);
            var enumerable = dashboards as Dashboard[] ?? dashboards.ToArray();
            var count = enumerable.Count();
            if (!canCreateDashBoards && (count == 0 || AllPanelsInvisible(enumerable))) {
                //either has no dashboard, or has only invisible panels due to roles
                //this user cannot create any dashboard, and there´s no one shared with him --> do not show the menu.
                return securedMenu;
            }


            if (!canCreateDashBoards && count == 1) {
                // if there´s just one dashboard to display and the user cannot create additionals, 
                // there´s no need to show a container, since there´s just one available action here.
                var action = new ActionMenuItemDefinition {
                    Controller = "Dashboard",
                    Action = "LoadPreferred",
                    Tooltip = "Click here to go to your preferred dashboard",
                    Id = "loadpreferred"

                };
                dashBoardMenu = action;
                user.Genericproperties[DashboardConstants.DashBoardsPreferredProperty] = enumerable.First().Id;
            } else if (canCreateDashBoards && count == 0) {
                var action = ManageDashboardAction();
                dashBoardMenu = action;
            } else {
                //TODO: make it selectable
                user.Genericproperties[DashboardConstants.DashBoardsPreferredProperty] = enumerable.First().Id;
                var container = new MenuContainerDefinition {
                    HasMainAction = true,
                    Action = "LoadPreferred",
                    Controller = "DashBoard",
                    Tooltip = "Click here to go to your preferred dashboard",
                    Id = "loadpreferred"

                };

                //SM - Don't add Manage Dashboard to submenu (click any of the save dashbaord will take you to the same place)
                //Maybe needed if we build a manage dashboard function

                // Only display manage dashboard option if user has access to do so
                //if (canCreateDashBoards) {
                //    var manageAction = ManageDashboardAction();
                //    container.AddLeaf(manageAction);
                //    container.AddLeaf(new DividerMenuItem());
                //}

                foreach (var dashboard in enumerable) {
                    var action = new ActionMenuItemDefinition {
                        Controller = "Dashboard",
                        Action = "LoadDashboard",
                        Parameters = new Dictionary<string, object> { { "dashboardid", dashboard.Id } },
                        Title = dashboard.Title,
                        Icon = "fa fa-bar-chart",
                    };
                    container.AddLeaf(action);
                }
                dashBoardMenu = container;
            }
            dashBoardMenu.Icon = "fa fa-tachometer";
            dashBoardMenu.Title = "Dashboard";
            leafs.Add(dashBoardMenu);
            leafs.AddRange(securedMenu.Leafs);
            securedMenu.Leafs = leafs;
            if (count > 0) {
                securedMenu.ItemindexId = "loadpreferred";
            }
            return securedMenu;
        }

        /// <summary>
        /// To create a dashboard, the user has to have the dashboard roles and at least one application should be visible to him
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static bool CanCreateDashBoards(ISWUser user) {
            return MetadataProvider.FetchTopLevelApps(ClientPlatform.Web, user).Any() && (user.IsInRole(DashboardConstants.RoleManager) || user.IsInRole(DashboardConstants.RoleAdmin));
        }

        private static bool AllPanelsInvisible(Dashboard[] enumerable) {
            return enumerable.All(d => d.Panels.Count == 0 || d.Panels.All(p => !p.Panel.Visible));
        }

        private static ActionMenuItemDefinition ManageDashboardAction() {
            var action = new ActionMenuItemDefinition {
                Controller = "Dashboard",
                Action = "Manage",
                Title = "Dashboard",
                Tooltip = "Click here to manage your dashboards"
            };
            return action;
        }
    }
}
