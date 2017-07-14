using System;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {
    [Class(Table = "OPT_DAILY_OUTAGE_MEETING", Lazy = false)]
    public class DailyOutageMeeting : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public DateTime? MeetingTime { get; set; }

        [Property]
        public string CriticalPath { get; set; }

        [Property]
        public string OpenActionItems { get; set; }

        [Property]
        public string CompletedActionItems { get; set; }

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
    }
}
