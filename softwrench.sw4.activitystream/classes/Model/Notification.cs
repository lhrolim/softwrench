using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.activitystream.classes.Model {
    public class Notification {
        private readonly string _application;
        private readonly string _targetSchema;
        private readonly string _id;
        private readonly string _label;
        private readonly string _icon;
        private readonly long _uId;
        private readonly string _parentId;
        private readonly long _parentUId;
        private readonly string _parentApplication;
        private readonly string _parentLabel;
        private readonly string _summary;
        private readonly string _createBy;
        internal readonly DateTime _notificationDate;
        private readonly long _rowstamp;
        private readonly string _flag;
        
        
        private bool _isRead;
        public string Flag { get { return _flag; } }

        public string Application { get { return _application; } }
        public string Id { get { return _id; } }
        public string TargetSchema { get { return _targetSchema; } }
        public string Label { get { return _label; } }
        public string Icon { get { return _icon; } }
        public long UId { get { return _uId; } }
        public string ParentId { get { return _parentId; } }
        public long ParentUId { get { return _parentUId; }}
        public string ParentApplication { get { return _parentApplication; } }
        public string ParentLabel { get { return _parentLabel; } }
        public string Summary { get { return _summary; } }
        public string CreateBy { get { return _createBy; } }
        public DateTime NotificationDate { get { return _notificationDate; } }
        public long Rowstamp { get { return _rowstamp; } }

        public bool IsRead { get; set; }

        public Notification(string application, string targetschema, string label, string icon, string id, long uid, string parentid, long parentuid, string parentApplication, string parentLabel, string summary, string createby, DateTime notificationdate, long rowstamp, string flag, bool isread = false) {
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
            _flag = flag;
            _isRead = isread;
        }
    }
}
