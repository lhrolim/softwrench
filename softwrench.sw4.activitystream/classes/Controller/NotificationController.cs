using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using cts.commons.web.Attributes;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sw4.activitystream.classes.Model;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softwrench.sw4.activitystream.classes.Controller {

    [Authorize]
    [SWControllerConfiguration]
    public class NotificationController : ApiController {

        private readonly NotificationFacade _notificationFacade;

        private static readonly ILog Log = LogManager.GetLogger(typeof(NotificationController));

        public NotificationController(NotificationFacade notificationFacade) {
            _notificationFacade = notificationFacade;
        }


        [HttpGet]
        public NotificationResponse GetNotifications() {
            var user = SecurityFacade.CurrentUser();
            var securityGroups = user.Profiles;
            if (securityGroups.Count == 0) {
                if (!user.IsSwAdmin()) {
                    Log.WarnFormat("User {0} with 0 security groups has logged into the system. No activity streams will be collected", user.Login);
                }
                //TODO: reconsider this strategy for future releases since, there´s a chance that 0 profiled users could see some grids (open ones), 
                // and hence it would make sense for them to see the activity streams; also, the permission could be set on the user level rather than the security profiles.
                return null;
            }
            var securityGroup = securityGroups.ElementAt(0);
            return _notificationFacade.GetNotificationStream(securityGroup.Name);
        }

        [HttpPost]
        public void UpdateNotificationReadFlag(string role, string application, string id, long rowstamp, bool isread = true) {
            _notificationFacade.UpdateNotificationReadFlag(role, application, id, rowstamp, isread);
        }

        [HttpPost]
        //Implementation to update read flag for multiple notifications
        public void UpdateNotificationReadFlag(string role, JArray ids, bool isread = true) {
            _notificationFacade.UpdateNotificationReadFlag(role, ids, isread);
        }

    }
}