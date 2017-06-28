using System;
using System.Globalization;
using cts.commons.portable.Util;
using cts.commons.Util;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {


    [Class(Table = "OPT_MAINTENANCE_ENG", Lazy = false)]
    public class MaintenanceEngineering : IFsEmailRequest {

        public const string ByStatusAndTime = "from MaintenanceEngineering where status in('Scheduled', 'Error') and sendTime <= ? and actualSendTime = null";
        public string ByToken => "from MaintenanceEngineering where Token = ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public string Engineer { get; set; }

        [JsonIgnore]
        [ManyToOne(NotNull = true, Column = "workpackageid", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public WorkPackage WorkPackage { get; set; }

        [Property]
        public DateTime? SendTime { get; set; }

        [Property]
        public DateTime? ActualSendTime { get; set; }


        [Property(Column = "status", TypeType = typeof(RequestStatusConverter))]
        public RequestStatus Status { get; set; }

        [Property]
        public string Reason { get; set; }

        [Property]
        public string Email { get; set; }

        [Property]
        public string Cc { get; set; }

        [Property]
        public bool SendNow { get; set; }

        [Property]
        public string Token { get; set; }

        [Property]
        public string Notes { get; set; }

        public string EntityDescription => "maintenance engineering request";

        public bool GenerateToken() {
            if (Token != null) {
                return false;
            }
            Token = TokenUtil.GenerateDateTimeToken();
            return true;
        }
    }
}
