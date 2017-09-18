using System;
using cts.commons.persistence;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {
    [Class(Table = "OPT_DAILY_OUTAGE_MEETING", Lazy = false)]
    public class DailyOutageMeeting : IFsEmailRequest {

        public string ByToken => "from DailyOutageMeeting where Token = ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [JsonIgnore]
        [ManyToOne(NotNull = true, Column = "workpackageid", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public WorkPackage WorkPackage { get; set; }

        [Property]
        public DateTime? MeetingTime { get; set; }

        [Property]
        public string CriticalPath { get; set; }

//        [Property]
//        public string OpenActionItems { get; set; }
//
//        [Property]
//        public string CompletedActionItems { get; set; }

        [Property]
        public string Cc { get; set; }

        [Property]
        public string Summary { get; set; }

        [Property]
        public decimal MWHLostYesterday { get; set; }

        /// <summary>
        /// "Sent" - daily outage meeting email sent
        /// "Error" - error during email send
        /// null - email not sent
        /// </summary>
        [Property(Column = "status", TypeType = typeof(RequestStatusConverter))]
        public RequestStatus? Status { get; set; }

        [Property]
        public bool SendNow { get; set; }

        [Property]
        public DateTime? ActualSendTime { get; set; }

        [Property]
        public string Token { get; set; }

        public string EntityDescription => "daily outage meeting";

        public bool GenerateToken() {
            if (Token != null) {
                return false;
            }
            Token = TokenUtil.GenerateDateTimeToken();
            return true;
        }

        public string Email { get; set; } // gfed emails are set here

        // Not used
        public string Notes { get; set; }
    }
}
