using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.api.classes;
using softwrench.sw4.api.classes.user;
using softwrench.sw4.dashboard.classes.model;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
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

            var leafs = new List<MenuBaseDefinition>();

            if (!user.Genericproperties.ContainsKey(DashboardConstants.DashBoardsProperty)) {
                //caching
                user.Genericproperties[DashboardConstants.DashBoardsProperty] = _userDashboardManager.LoadUserDashboars(user);
            }

            var dashboards = user.Genericproperties[DashboardConstants.DashBoardsProperty] as IEnumerable<Dashboard>;


            MenuBaseDefinition dashBoardMenu;

            var canCreateDashBoards = user.IsInRole(DashboardConstants.RoleManager) || user.IsInRole(DashboardConstants.RoleAdmin);
            var enumerable = dashboards as Dashboard[] ?? dashboards.ToArray();
            var count = enumerable.Count();
            if (!canCreateDashBoards && count == 0) {
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
                //var manageAction = ManageDashboardAction();
                //container.AddLeaf(manageAction);
                container.AddLeaf(new DividerMenuItem());
                foreach (var dashboard in enumerable) {
                    var action = new ActionMenuItemDefinition {
                        Controller = "Dashboard",
                        Action = "LoadDashboard",
                        Parameters = new Dictionary<string, object> { { "dashboardid", dashboard.Id } },
                        Title = dashboard.Title,
                    };
                    container.AddLeaf(action);
                }
                dashBoardMenu = container;
            }
            dashBoardMenu.Icon = "fa fa-home";
            leafs.Add(dashBoardMenu);
            leafs.AddRange(securedMenu.Leafs);
            securedMenu.Leafs = leafs;
            securedMenu.ItemindexId = "loadpreferred";
            return securedMenu;
        }

        private static ActionMenuItemDefinition ManageDashboardAction() {
            var action = new ActionMenuItemDefinition {
                Controller = "Dashboard",
                Action = "Manage",
                Title = "Manage Dashboards",
                Tooltip = "Click here to manage your dashboards"
            };
            return action;
        }
    }
}
