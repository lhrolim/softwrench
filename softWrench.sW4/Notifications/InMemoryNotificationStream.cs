using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Hql.Ast.ANTLR;
using softWrench.sW4.Notifications.Entities;

namespace softWrench.sW4.Notifications {
    class InMemoryNotificationStream {
        private List<Notification> _notifications;
        private readonly int _hoursToPurge;

        public int HoursToPurge { get { return _hoursToPurge; } }

        public InMemoryNotificationStream(int hoursToPurge) {
            _notifications = new List<Notification>();
            _hoursToPurge = hoursToPurge;
        }

        public void InsertNotificationIntoStream(Notification notification) {
            _notifications.Add(notification);
        }

        public void PurgeNotificationsFromStream() {
            _notifications.RemoveAll(x => DateTime.Now > x.NotificationDate.AddSeconds(-3600*HoursToPurge));
        }

        public void UpdateNotificationHiddenFlag(string application, string id, bool isHidden)
        {
            var notificationsToUpdate = (from n in _notifications
                where n.Application == application &&
                      n.Id == id
                select n);

            foreach (var notification in notificationsToUpdate) {
                notification.IsHidden = isHidden;

                var childNotificationsToUpdate = (from n in _notifications
                    where n.ParentId == id &&
                          n.Application == application
                    select n);

                foreach (var childNotification in childNotificationsToUpdate) {
                    UpdateNotificationHiddenFlag(application, childNotification.Id, isHidden);
                }
            }
        }
    }
}
