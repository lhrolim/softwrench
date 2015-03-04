using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Hql.Ast.ANTLR;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.Notifications.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Notifications {
    public class InMemoryNotificationStream {
        private List<Notification> _notifications;
        
        public InMemoryNotificationStream() {
            _notifications = new List<Notification>();
        }

        public List<Notification> GetNotifications() {
            _notifications.Sort((n1, n2) => n2.NotificationDate.CompareTo(n1.NotificationDate));
            _notifications =HandleChildNotifications(_notifications);
            //List<Notification> Notifications = (from notifications in _notifications
            //    select new Notification(notifications.Application,
            //        notifications.TargetSchema,
            //        notifications.Label,
            //        notifications.Icon,
            //        notifications.Id,
            //        notifications.UId,
            //        notifications.ParentApplication,
            //        notifications.ParentUId,
            //        notifications.ParentApplication,
            //        notifications.ParentLabel,
            //        notifications.Summary,
            //        notifications.CreateBy,
            //        notifications.NotificationDate.FromMaximoToUser(SecurityFacade.CurrentUser()),
            //        notifications.Rowstamp,
            //        notifications.Flag,
            //        notifications.IsRead)
            //    ).ToList();
            return _notifications;
        }

        private List<Notification> HandleChildNotifications(List<Notification> _notifications){
            int i,j = 0;
            
            for ( i = 0;i < _notifications.Count; i++){
                for (j = 1; j < _notifications.Count; j++){
                    if (i != j) { 
                        if(_notifications.ElementAt(i).NotificationDate.Equals(_notifications.ElementAt(j).NotificationDate) && _notifications.ElementAt(i).NotificationDate.Ticks.Equals(_notifications.ElementAt(j).NotificationDate.Ticks)){
                            if (_notifications.ElementAt(i).ParentId == null){
                                _notifications.RemoveAt(i);
                            }
                        }
                    }
                }
            }

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
