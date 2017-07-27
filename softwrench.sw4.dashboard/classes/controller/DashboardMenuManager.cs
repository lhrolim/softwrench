using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using log4net;
using softwrench.sw4.api.classes;
using softwrench.sw4.api.classes.user;
using softwrench.sw4.dashboard.classes.model;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.dashboard.classes.controller {
    public class DashboardMenuManager : IMenuManager {

        private readonly UserDashboardManager _userDashboardManager;

        private static readonly ILog Log = LogManager.GetLogger(typeof(DashboardMenuManager));

        private static readonly ActionMenuItemDefinition ManageDashboardAction = new ActionMenuItemDefinition {
            Controller = "Dashboard",
            Action = "Manage",
            Title = "Dashboard",
            Tooltip = "Click here to manage your dashboards"
        };


        public DashboardMenuManager(UserDashboardManager userDashboardManager) {
            _userDashboardManager = userDashboardManager;
            Log.Debug("starting log");
        }

        public MenuDefinition ModifyMenu(MenuDefinition securedMenu, ISWUser user) {
            //if (!ApplicationConfiguration.IsLocal()) {
            //    //TODO: remove this when it´s good for qa
            //    return securedMenu;
            //}

            // If the dashboards are not enabled, do not load the dashboards.
            if ("false".Equals(MetadataProvider.GlobalProperty("dashboard.enabled"))) {
                Log.DebugFormat("dashboard application is turned off returning");
                return securedMenu;
            }

            var leafs = new List<MenuBaseDefinition>();

            if (!user.Genericproperties.ContainsKey(DashboardConstants.DashBoardsProperty)) {
                //caching
                user.Genericproperties[DashboardConstants.DashBoardsProperty] = _userDashboardManager.LoadUserDashboars((InMemoryUser) user);
            }

            var dashboards = user.Genericproperties[DashboardConstants.DashBoardsProperty] as IEnumerable<Dashboard>;


            MenuBaseDefinition dashBoardMenu;

            var canCreateDashBoards = CanCreateDashBoards(user);
            var enumerable = dashboards as Dashboard[] ?? dashboards.ToArray();
            var count = enumerable.Count();
            if (!canCreateDashBoards && (count == 0 || AllPanelsInvisible(enumerable))) {
                Log.DebugFormat("User {0} cannot create dashboards and there is no visible panel, returning", UserName(user));
                //either has no dashboard, or has only invisible panels due to roles
                //this user cannot create any dashboard, and there´s no one shared with him --> do not show the menu.
                return securedMenu;
            }


            if (!canCreateDashBoards && count == 1) {
                Log.DebugFormat("User {0} cannot create dashboards, showing visibile one", UserName(user));
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
                Log.DebugFormat("No visible dashboards but user {0} can create them", UserName(user));
                var action = ManageDashboardAction;
                dashBoardMenu = action;
            } else {
                Log.DebugFormat("Adding user {0} dashboards", UserName(user));

                //TODO: make it selectable
                user.Genericproperties[DashboardConstants.DashBoardsPreferredProperty] = enumerable.First().Id;
                var container = new MenuContainerDefinition {
                    HasMainAction = true,
                    Action = "LoadPreferred",
                    Controller = "DashBoard",
                    Tooltip = "Click here to go to your preferred dashboard",
                    Id = "loadpreferred"

                };

                foreach (var dashboard in enumerable) {
                    var action = new ActionMenuItemDefinition {
                        Controller = "Dashboard",
                        Action = "LoadDashboard",
                        Parameters = new Dictionary<string, object> { { "dashboardid", dashboard.Id } },
                        Title = dashboard.Title,
                        Icon = "fa fa-bar-chart",
                    };
                    Log.DebugFormat("Adding dashboard {0}", UserName(user));
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
        /// To create a dashboard, the user has to have the dashboard roles
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static bool CanCreateDashBoards(ISWUser user) {
            return  (user.IsInRole(DashboardConstants.RoleManager) || user.IsInRole(DashboardConstants.RoleAdmin));
        }

        private static bool AllPanelsInvisible(Dashboard[] enumerable) {
            return enumerable.All(d => d.Panels.Count == 0 || d.Panels.All(p => !p.Panel.Visible));
        }

        private static string UserName(IPrincipal user) {
            return user?.Identity != null ? user.Identity.Name : "Anonymous";
        }
    }
}
