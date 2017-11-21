using System;
using System.Collections.Generic;
using cts.commons.persistence;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using softwrench.sw4.batch.api.entities;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.Entities.Attachment;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model {

    [Class(Table = "DISP_TICKET", Lazy = false)]
    public class DispatchTicket : IBaseEntity {
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
        public string SupportPhone { get; set; }

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


        [Bag(0, Table = "DISP_INVERTER", Cascade = "all", Lazy = CollectionLazy.False, Inverse = true, OrderBy = "AssetNum asc")]
        [Key(1, Column = "ticketid", NotNull = true)]
        [OneToMany(2, ClassType = typeof(Inverter))]
        public virtual IList<Inverter> Inverters { get; set; }

        [Bag(0, Table = "SW_DOCLINK", Cascade = "all", Lazy = CollectionLazy.False, Where = "OwnerTable = '_DispatchTicket' ", Inverse = true)]
        [Key(1, Column = "ownerid")]
        [OneToMany(2, ClassType = typeof(DocLink))]
        public virtual IList<DocLink> Attachments { get; set; }
    }


    class DispatchTicketType : EnumStringType<DispatchTicketStatus> {
    }

}
