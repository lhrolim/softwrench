using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.Entities.Attachment;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model {

    [Class(Table = "DISP_TICKET", Lazy = false)]
    public class DispatchTicket : IBaseEntity {
        public const string ByToken = "from DispatchTicket where AccessToken = ?";

        public const string EscalationQuery = "from DispatchTicket where Status in ('Scheduled','Dispatched') and DispatchExpectedDate is not null and DispatchExpectedDate < ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [Property(TypeType = typeof(DispatchTicketType))]
        public DispatchTicketStatus Status { get; set; }

        [Property]
        public DateTime? StatusDate { get; set; }

        [ManyToOne(Column = "statusreportedby", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public User StatusReportedBy { get; set; }


        [ManyToOne(Column = "reportedby", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public User ReportedBy { get; set; }


        [Property]
        public long GfedId { get; set; }

        [Property]
        public DateTime? CreatedDate { get; set; }

        [Property]
        public string SiteAddress { get; set; }

        [Property]
        public string SiteContact { get; set; }

        [Property]
        public string SiteContactPhone { get; set; }

        [Property]
        public string MaintenaceProvider { get; set; }

        [Property]
        public string SupportPhone { get; set; }

        [Property]
        public string SupportEmail { get; set; }

        [Property]
        public string PrimaryContact { get; set; }

        [Property]
        public string PrimaryContactPhone { get; set; }

        [Property]
        public string EscalationContact { get; set; }

        [Property]
        public string EscalationContactPhone { get; set; }

        [Property]
        public decimal? GpsLatitude { get; set; }

        [Property]
        public decimal? GpsLongitude { get; set; }

        [Property]
        public string Comments { get; set; }

        [Property]
        public bool ImmediateDispatch { get; set; }

        [Property]
        public DateTime? DispatchExpectedDate { get; set; }

        [Property]
        public DateTime? ArrivedTime { get; set; }

        [Property]
        public DateTime? LastSent { get; set; }


        [Bag(0, Table = "DISP_INVERTER", Cascade = "all", Lazy = CollectionLazy.False, Inverse = true,
            OrderBy = "AssetNum asc")]
        [Key(1, Column = "ticketid", NotNull = true)]
        [OneToMany(2, ClassType = typeof(Inverter))]
        public virtual IList<Inverter> Inverters { get; set; } = new List<Inverter>();

        [Bag(0, Table = "SW_DOCLINK", Cascade = "all", Lazy = CollectionLazy.False, Where = "OwnerTable = '_DispatchTicket' ", Inverse = true)]
        [Key(1, Column = "ownerid")]
        [OneToMany(2, ClassType = typeof(DocLink))]
        public virtual IList<DocLink> Attachments { get; set; } = new List<DocLink>();

        /// <summary>
        /// Token used to change status without requiring the user to be authenticated
        /// </summary>
        [Property]
        public string AccessToken { get; set; }


        public int CalculateHours() {
            if (!DispatchExpectedDate.HasValue) {
                return 0;
            }

            var ts = DateTime.Now - DispatchExpectedDate.Value;
            return (int)ts.TotalHours;
        }

        public int CalculateLastSentHours() {
            if (!LastSent.HasValue || !DispatchExpectedDate.HasValue) {
                return int.MinValue;
            }

            var ts = LastSent.Value - DispatchExpectedDate.Value;
            return (int)ts.TotalHours;
        }

        public double CalculateCountDown() {
            if (!DispatchExpectedDate.HasValue) {
                return 0;
            }

            var hasClass1 = Inverters.Any(i => "1".EqualsIc(i.FailureClass));

            var countDownHours = hasClass1 ? 24 : 48;


            var limit = DispatchExpectedDate.Value.AddHours(countDownHours);
            // if the technician has arrived the clock is stopped at the arrived time, 
            // and we´ll get to know how many hours left he still had
            var baseTime = ArrivedTime ?? DateTime.Now;

            var totalTimeSpan = limit - baseTime;
            return totalTimeSpan.TotalHours;
        }

        public string OnSiteDeadLine() {
            var deadLineHours = Inverters?.FirstOrDefault(inverter => "1".Equals(inverter.FailureClass)) == null ? 48 : 24;
            return FmtDateTime(DispatchExpectedDate?.Add(new TimeSpan(0, deadLineHours, 0, 0)));
        }

        public string FmtDateTime(DateTime? date) {
            return BaseFmtDate(date, "G");
        }

        public string FmtDate(DateTime? date) {
            return BaseFmtDate(date, "MM/dd/yy");
        }

        private static string BaseFmtDate(DateTime? date, string format) {
            if (date == null) {
                return "";
            }
            var culture = new CultureInfo("en-US");
            return date.Value.ToString(format, culture);
        }

        public string EmailMemoryKey() {
            return "email"+ this.GetType().Name + Id;
        }


    }


    class DispatchTicketType : EnumStringType<DispatchTicketStatus> {
    }

}
