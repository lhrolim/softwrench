using System;
using System.Globalization;
using cts.commons.portable.Util;
using cts.commons.Util;
using NHibernate.Mapping.Attributes;

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

        [Property]
        public int WorkPackageId { get; set; }

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

            var token = "" + new Random(100).Next(10000);
            token += DateTime.Now.TimeInMillis().ToString(CultureInfo.InvariantCulture);
            token += AuthUtils.GetSha1HashData(token);
            Token = token;
            return true;
        }
    }
}
