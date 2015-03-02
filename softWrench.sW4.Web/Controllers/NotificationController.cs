using System;
using System.Net;
using NHibernate.Mapping;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Notifications;
using softWrench.sW4.Notifications.Entities;
using softwrench.sW4.Shared2.Metadata;
using softWrench.sW4.Util;
using softWrench.sW4.Security.Services;
using System.Web.Http;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata.Applications.DataSet;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Data.API;
using System.Collections.Generic;
using System.Web.Http;

namespace softWrench.sW4.Web.Controllers {

    public class NotificationController : ApiController
    {
        [HttpGet]
        public List<Notification> GetNotifications(string role) {
            var notificationFacade = NotificationFacade.GetInstance();
            return notificationFacade.GetNotificationStream(role);
        }

        [HttpPost]
        public void UpdateNotificationReadFlag(string role, string application, string id, long rowstamp, bool isread = true) {
            var notificationFacade = NotificationFacade.GetInstance();
            notificationFacade.UpdateNotificationReadFlag(role, application, id, rowstamp, isread);
        }

        [HttpPost]
        //Implementation to update read flag for multiple notifications
        public void UpdateNotificationReadFlag(string role, JArray ids, bool isread = true) {
            var notificationFacade = NotificationFacade.GetInstance();
            notificationFacade.UpdateNotificationReadFlag(role, ids, isread);
        }

    }
}