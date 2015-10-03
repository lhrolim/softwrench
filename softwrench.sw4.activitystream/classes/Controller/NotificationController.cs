using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using cts.commons.portable.Util;
using cts.commons.web.Attributes;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sw4.activitystream.classes.Model;
using softwrench.sw4.user.classes.entities;
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
        public NotificationResponse GetNotifications([FromUri]int? currentProfile) {
            var user = SecurityFacade.CurrentUser();
            var securityGroups = user.Profiles;
            var profileDTO = _notificationFacade.GetNotificationProfile(currentProfile,securityGroups);
            var securityGroup = profileDTO.SelectedProfile;

            var notificationResponse = _notificationFacade.GetNotificationStream(securityGroup.Name);
            if (notificationResponse != null) {
                notificationResponse.AvailableProfiles = profileDTO.AvailableProfiles;
                notificationResponse.SelectedProfile = securityGroup.Id;
            }

            return notificationResponse;
        }

        [HttpPost]
        public void UpdateNotificationReadFlag(int? securityGroup, string application, string id, long rowstamp, bool isread = true) {
            _notificationFacade.UpdateNotificationReadFlag(securityGroup, application, id, rowstamp, isread);
        }

        [HttpPost]
        //Implementation to update read flag for multiple notifications
        public void UpdateNotificationReadFlag(int? securityGroup, JArray ids, bool isread = true) {
            _notificationFacade.UpdateNotificationReadFlag(securityGroup, ids, isread);
        }

    }
}