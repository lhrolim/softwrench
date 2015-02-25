﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Notifications.Entities {
    public class Notification {
        private readonly string _application;
        private readonly string _targetSchema;
        private readonly string _id;
        private readonly string _label;
        private readonly string _icon;
        private readonly int _uId;
        private readonly string _parentId;
        private readonly int _parentUId;
        private readonly string _parentApplication;
        private readonly string _parentLabel;
        private readonly string _summary;
        private readonly string _createBy;
        private readonly DateTime _notificationDate;
        private readonly long _rowstamp;
        
        
        private bool _isRead;

        public string Application { get { return _application; } }
        public string Id { get { return _id; } }
        public string TargetSchema { get { return _targetSchema; } }
        public string Label { get { return _label; } }
        public string Icon { get { return _icon; } }
        public int UId { get { return _uId; } }
        public string ParentId { get { return _parentId; } }
        public int ParentUId { get { return _parentUId; }}
        public string ParentApplication { get { return _parentApplication; } }
        public string ParentLabel { get { return _parentLabel; } }
        public string Summary { get { return _summary; } }
        public string CreateBy { get { return _createBy; } }
        public DateTime NotificationDate { get { return _notificationDate; } }
        public long Rowstamp { get { return _rowstamp; } }

        
        public bool IsRead { get { return _isRead; } set { _isRead = value; }}

        public Notification(string application, string targetschema, string label, string icon, string id, int uid, string parentid, int parentuid, string parentApplication, string parentLabel, string summary, string createby, DateTime notificationdate, long rowstamp, bool isread = false) {
            _application = application;
            _id = id;
            _targetSchema = targetschema;
            _label = label;
            _icon = icon;
            _uId = uid;
            _parentId = parentid;
            _parentUId = parentuid;
            _parentApplication = parentApplication;
            _parentLabel = parentLabel;
            _summary = summary;
            _createBy = createby;
            _notificationDate = notificationdate;
            _rowstamp = rowstamp;
           
            _isRead = isread;
        }
    }
}
