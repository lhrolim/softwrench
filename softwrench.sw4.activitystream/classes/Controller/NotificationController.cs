using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using cts.commons.web.Attributes;
using Newtonsoft.Json.Linq;
using softwrench.sw4.activitystream.classes.Model;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softwrench.sw4.activitystream.classes.Controller {

    [Authorize]
    [SWControllerConfiguration]
    public class NotificationController : ApiController {

        private readonly NotificationFacade _notificationFacade;

        public NotificationController(NotificationFacade notificationFacade) {
            _notificationFacade = notificationFacade;
        }


        [HttpGet]
        public NotificationResponse GetNotifications() {
            var securityGroups = SecurityFacade.CurrentUser().Profiles;
            if (securityGroups.Count == 0) {
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