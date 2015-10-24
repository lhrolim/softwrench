namespace softwrench.sw4.dashboard.classes.model {
    internal class DashboardConstants {

        /// <summary>
        /// Allows the creation/removal of other shareable dashboards for the entire organization, plus personal ones
        /// </summary>
        public static string RoleAdmin = "ROLE_DASH_ADMIN";

        /// <summary>
        /// Allows the creation of personal dashboards, but do not allow the creation/removal of eventual shared ones
        /// </summary>
        public static string RoleManager = "ROLE_DASH_MANAGER";

        /// <summary>
        /// User generic Property to hold its dashboards
        /// </summary>
        public static string DashBoardsProperty = "dash_dashboards";

        /// <summary>
        /// User generic Property to hold his preferred dashboard
        /// </summary>
        public static string DashBoardsPreferredProperty = "dash_preffereddashboard";

        internal static class PanelTypes {
            public const string Grid = "dashboardgrid";
            public const string Graphic = "dashboardgraphic";
        }

    }
}
