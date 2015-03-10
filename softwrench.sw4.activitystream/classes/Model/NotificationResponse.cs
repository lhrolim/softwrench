using System.Collections.Generic;

namespace softwrench.sw4.activitystream.classes.Model
{
    public class NotificationResponse
    {
        private readonly int _readCount;
        private readonly List<Notification> _notifications;

        public int ReadCount { get { return _readCount; } }
        public List<Notification> Notifications { get { return _notifications; } } 

        public NotificationResponse(List<Notification> notifications, int readCount)
        {
            _readCount = readCount;
            _notifications = notifications;
        }
    }
}