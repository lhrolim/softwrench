using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Notifications;
using System.Collections.Concurrent;
using softWrench.sW4.Notifications.Entities;

namespace softWrench.sW4.Notifications {
    class NotificationFacade {

        private static NotificationFacade _instance = null;

        public static readonly IDictionary<string, InMemoryNotificationStream> _notificationStreams = new ConcurrentDictionary<string, InMemoryNotificationStream>();

        //Sets up the default notification stream.
        public static void InitNotificationStreams() {
            var hoursToPurge = 24;
            InMemoryNotificationStream allRoleNotificationBuffer = new InMemoryNotificationStream(hoursToPurge);
            _notificationStreams["allRole"] = allRoleNotificationBuffer;
        }

        public static InMemoryNotificationStream CurrentNotificationStream() {
            return _notificationStreams["allRole"];
        }

        public static NotificationFacade GetInstance() {
            if (_instance == null) {
                _instance = new NotificationFacade();
            }
            return _instance;
        }

        //Currently only inserts notifications into the 'allRole' stream.
        //This would need to be updated in the future to determine which
        //role stream needs to be inserted based on which roles have a
        //notificationstream attribute set to true
        public void InsertNotificationsIntoStreams(Iesi.Collections.Generic.ISet<Notification> notifications) {
            var streamToUpdate = _notificationStreams["allRole"];
            foreach (var notification in notifications) {
                streamToUpdate.InsertNotificationIntoStream(notification);
            }
        }

        //Currently only updates notifications into the 'allRole' stream.
        //This would need to be updated in the future to determine which
        //role stream needs to be updated based on which roles have a
        //notificationstream attribute set to true
        public void UpdateNotificationAsHidden(string role, string application, string id) {
            var streamToUpdate = _notificationStreams[role];
            streamToUpdate.UpdateNotificationHiddenFlag(application, id, true);
        }
    }
}
