using System;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {


    [Class(Table = "OPT_MAINTENANCE_ENG", Lazy = false)]
    public class MaintenanceEngineering : IFsEmailRequest {

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
        public bool SendNow { get; set; }

        [Property]
        public string Token { get; set; }

        [Property]
        public string Notes { get; set; }

        public string EntityDescription => "maintenance engineering request";
    }
}
