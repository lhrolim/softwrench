﻿using System.Collections.Generic;
using softWrench.sW4.Util;

namespace softwrench.sw4.activitystream.classes.Model
{
    public class NotificationResponse
    {
        private readonly int _readCount;
        private readonly string _refreshRate;
        private readonly List<Notification> _notifications;

        public int ReadCount { get { return _readCount; } }
        public List<Notification> Notifications { get { return _notifications; } }
        public string RefreshRate { get { return _refreshRate; } }

        public NotificationResponse(List<Notification> notifications, int readCount)
        {
            _refreshRate = ApplicationConfiguration.ActivityStreamRefreshRate;
            _readCount = readCount;
            _notifications = notifications;
        }
    }
}