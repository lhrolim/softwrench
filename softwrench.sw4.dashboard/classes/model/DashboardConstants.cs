namespace softwrench.sw4.dashboard.classes.model {
    internal class DashboardConstants
    {

        /// <summary>
        /// Allows the creation/removal of other shareable dashboards for the entire organization, plus personal ones
        /// </summary>
        public static string RoleAdmin = "ROLE_DASH_ADMIN";

        /// <summary>
        /// Allows the creation of personal dashboards, but do not allow the creation/removal of eventual shared ones
        /// </summary>
        public static string RoleManager = "ROLE_DASH_MANAGER";

    }
}
