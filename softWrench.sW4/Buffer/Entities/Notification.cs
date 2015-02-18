using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Buffer.Entities {
    class Notification {
        private string id;
        private string parentId;
        private string summary;
        private string createBy;
        private DateTime notificationDate;
        private int rowstamp;

        public string Id { get { return id; } };
        public string ParentId { get; set; };
        public string Summary { get; set; };
        public string CreateBy { get; set; };
        public DateTime NotificationDate { get; set; };
        public string Rowstamp { get; set; };
    }
}
