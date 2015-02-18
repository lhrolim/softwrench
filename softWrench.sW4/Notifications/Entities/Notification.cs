using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Notifications.Entities {
    public class Notification {
        private readonly string _application;
        private readonly string _id;
        private readonly string _parentId;
        private readonly string _summary;
        private readonly string _createBy;
        private readonly DateTime _notificationDate;
        private readonly long _rowstamp;
        
        private bool _isHidden;
        private bool _isRead;

        public string Application { get { return _application; } }
        public string Id { get { return _id; } }
        public string ParentId { get { return _parentId; } }
        public string Summary { get { return _summary; } }
        public string CreateBy { get { return _createBy; } }
        public DateTime NotificationDate { get { return _notificationDate; } }
        public long Rowstamp { get { return _rowstamp; } }

        public bool IsHidden { get { return _isHidden; } set { _isHidden = value; }}
        public bool IsRead { get { return _isRead; } set { _isRead = value; }}

        public Notification(string application, string id, string parentid, string summary, string createby, DateTime notificationdate, long rowstamp, bool ishidden = false, bool isread = false) {
            _application = application;
            _id = id;
            _parentId = parentid;
            _summary = summary;
            _createBy = createby;
            _notificationDate = notificationdate;
            _rowstamp = rowstamp;
            _isHidden = ishidden;
            _isRead = isread;
        }
    }
}
