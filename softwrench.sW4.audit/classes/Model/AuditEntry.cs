using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sW4.audit.classes.Model {
    public class AuditEntry
    {
        private readonly long _id;
        private readonly string _action;
        private readonly string _refApplication;
        private readonly long _refId;
        private readonly string _data;
        private readonly string _createBy;
        private readonly DateTime _createDate;

        public long Id { get { return _id; } }
        public string Action { get { return _action; } }
        public string RefApplication { get { return _refApplication; } }
        public long RefId { get { return _refId; } }
        public string Data { get { return _data; } }
        public string CreateBy { get { return _createBy; } }
        public DateTime CreateDate { get { return _createDate; } }
    }
}
