using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Hql.Ast.ANTLR;
using softWrench.sW4.Notifications.Entities;

namespace softWrench.sW4.Notifications {
    public class InMemoryNotificationStream {
        private List<Notification> _notifications;
        
        public InMemoryNotificationStream() {
            _notifications = new List<Notification>();
        }

        public List<Notification> GetNotifications() {
            _notifications.Sort((n1, n2) => n2.NotificationDate.CompareTo(n1.NotificationDate));
            return _notifications;
        }

        public void InsertNotificationIntoStream(Notification notification)
        {
            //Checks if record already exists in stream
            var existingNotifications = _notifications.Any(n => n.Id == notification.Id && n.Application == notification.Application && n.NotificationDate == notification.NotificationDate);
            
            if (!existingNotifications) {
                _notifications.Add(notification);    
            }
        }

        public void PurgeNotificationsFromStream(int hoursToPurge){

            List<Notification> oldNotifications = _notifications.FindAll(x => DateTime.Now.AddSeconds(-3600*hoursToPurge) > x.NotificationDate).Take(20).ToList(); 
            _notifications.RemoveAll(x => DateTime.Now.AddSeconds(-3600 * hoursToPurge) > x.NotificationDate);
            //Keep top 20 from the past
            if (_notifications.Count < 20){
                _notifications.AddRange(oldNotifications.Take(20-_notifications.Count));
            }
        }

        public void UpdateNotificationReadFlag(string application, string id, bool isRead)
        {
            var notificationsToUpdate = (from n in _notifications
                                         where n.Application == application &&
                                               n.Id == id
                                         select n);

            foreach (var notification in notificationsToUpdate) {
                notification.IsRead = isRead;
            }
        }
    }
}
