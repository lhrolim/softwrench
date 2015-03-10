using System;
using System.Collections.Generic;
using System.Linq;

namespace softwrench.sw4.activitystream.classes.Model {
    public class InMemoryNotificationStream {
        private List<Notification> _notifications;
        
        public InMemoryNotificationStream() {
            _notifications = new List<Notification>();
        }

        public List<Notification> GetNotifications() {
            _notifications = HandleChildNotifications(_notifications);
            List<Notification> orderedNotifications = _notifications.OrderBy(n => n.IsRead).ThenByDescending(n => n.NotificationDate).ToList();
            
            //List<Notification> Notifications = (from notifications in _notifications
            //                                    select new Notification(notifications.Application,
            //                                        notifications.TargetSchema,
            //                                        notifications.Label,
            //                                        notifications.Icon,
            //                                        notifications.Id,
            //                                        notifications.UId,
            //                                        notifications.ParentApplication,
            //                                        notifications.ParentUId,
            //                                        notifications.ParentApplication,
            //                                        notifications.ParentLabel,
            //                                        notifications.Summary,
            //                                        notifications.CreateBy,
            //                                        notifications.NotificationDate.FromMaximoToUser(SecurityFacade.CurrentUser()),
            //                                        notifications.Rowstamp,
            //                                        notifications.Flag,
            //                                        notifications.IsRead)
            //    ).ToList();
            return orderedNotifications;
        }

        private List<Notification> HandleChildNotifications(List<Notification> notifications){
            notifications.RemoveAll(parent => notifications.Any(child => child.ParentUId == parent.UId && child.NotificationDate.Ticks.Equals(parent.NotificationDate.Ticks)));

            return notifications;
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

        public void UpdateNotificationReadFlag(string application, string id, long rowstamp, bool isRead)
        {
            var notificationsToUpdate = (from n in _notifications
                                         where n.Application == application &&
                                               n.Id == id &&
                                               n.Rowstamp.Equals(rowstamp)
                                         select n);

            foreach (var notification in notificationsToUpdate) {
                notification.IsRead = isRead;
            }
        }
    }
}
