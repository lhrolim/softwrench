using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration {

    public class FirstSolarSecurityGroupSetup : ISWEventListener<ApplicationStartedEvent> {

        private static string DailyOutageGroup = "DailyOutage";

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public UserProfileManager ProfileManager { get; set; }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            var profile = ProfileManager.FindByName(DailyOutageGroup);
            if (profile != null) {
                return;
            }

            var group = new UserProfile {
                Name = DailyOutageGroup,
                Description = "Daily Outage Security Group",
                Deletable = false
            };

            var appPermission = new ApplicationPermission
            {
                ApplicationName = "_WorkPackage",
                AllowCreation = true,
                AllowUpdate = true
            };


            appPermission.ContainerPermissions = new HashSet<ContainerPermission>();

            group.ApplicationPermissions = new HashSet<ApplicationPermission>();


        }
    }
}
