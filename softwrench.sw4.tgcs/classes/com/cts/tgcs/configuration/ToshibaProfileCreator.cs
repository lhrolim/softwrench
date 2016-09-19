using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using Iesi.Collections.Generic;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.configuration {

    public class ToshibaProfileRegistry : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        public const string QsrpRofile = "QuickSRAccess";
        public const string SrpRofile = "SRAccess";


        private readonly UserProfileManager _userProfileManager;

        public ToshibaProfileRegistry(UserProfileManager userProfileManager) {
            _userProfileManager = userProfileManager;
        }


        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            var qsrProfileObject = _userProfileManager.FindByName(QsrpRofile);

            if (qsrProfileObject == null) {
                var profile = new UserProfile {
                    Name = QsrpRofile,
                    Description = "Profile to allow Quick Service request Access",
                    ApplicationPermissions = new LinkedHashSet<ApplicationPermission>()
                };
                var appPermission = new ApplicationPermission();
                appPermission.AllowCreation = appPermission.AllowUpdate = true;
                appPermission.ApplicationName = "quickservicerequest";
                profile.ApplicationPermissions.Add(appPermission);
                _userProfileManager.SaveUserProfile(profile);
            }

            var srProfileObject = _userProfileManager.FindByName(SrpRofile);

            if (srProfileObject == null) {
                var profile = new UserProfile {
                    Name = SrpRofile,
                    Description = "Profile to allow Service request Access",
                    ApplicationPermissions = new LinkedHashSet<ApplicationPermission>()
                };
                var appPermission = new ApplicationPermission();
                appPermission.AllowCreation = appPermission.AllowUpdate = true;
                appPermission.ApplicationName = "servicerequest";
                profile.ApplicationPermissions.Add(appPermission);
                _userProfileManager.SaveUserProfile(profile);
            }

        }
    }
}
