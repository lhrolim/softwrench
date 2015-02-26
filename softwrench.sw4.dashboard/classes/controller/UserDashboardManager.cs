using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.simpleinjector;
using softwrench.sw4.api.classes.user;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Data.Persistence.SWDB;

namespace softwrench.sw4.dashboard.classes.controller {
    public class UserDashboardManager : ISingletonComponent {

        private readonly SWDBHibernateDAO _dao;

        public UserDashboardManager(SWDBHibernateDAO dao) {
            _dao = dao;
        }

        public IEnumerable<Dashboard> LoadUserDashboars(ISWUser currentUser) {

            var profiles = currentUser.ProfileIds;
            var enumerable = profiles as int?[] ?? profiles.ToArray();
            if (enumerable.Any()) {
                var sb = BuildUserProfileColumn(enumerable);
                return _dao.FindByQuery<Dashboard>(Dashboard.ByUser, currentUser.UserId, sb.ToString());
            }
            return _dao.FindByQuery<Dashboard>(Dashboard.ByUserNoProfile, currentUser.UserId);


        }

        private static StringBuilder BuildUserProfileColumn(int?[] enumerable) {
            var sb = new StringBuilder("%");
            foreach (var profile in enumerable) {
                sb.Append(";").Append(profile).Append(";");
            }
            sb.Append("%");
            return sb;
        }

        public IEnumerable<DashboardBasePanel> LoadUserPanels(ISWUser currentUser, string panelType) {
            var profiles = currentUser.ProfileIds;
            var enumerable = profiles as int?[] ?? profiles.ToArray();
            string entityName = typeof(DashboardGridPanel).Name;
            switch (panelType) {
                case "dashboardgrid":
                    entityName = typeof(DashboardGridPanel).Name;
                    break;
                case "":
                    //                    entityName = "dashboardgraphic";
                    throw new NotSupportedException("graphics are not yet implemented");
                    break;
            }

            if (enumerable.Any()) {
                var sb = BuildUserProfileColumn(enumerable);
                return _dao.FindByQuery<DashboardBasePanel>(DashboardBasePanel.ByUser(entityName), currentUser.UserId, sb.ToString());
            }
            return _dao.FindByQuery<DashboardBasePanel>(DashboardBasePanel.ByUserNoProfile(entityName), currentUser.UserId);
        }
    }
}
