using System;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {


    [Class(Table = "OPT_CALLOUT", Lazy = false)]
    public class CallOut {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public long WorkPackageId { get; set; }

        //actually ONETOONE, but who cares, since NHIB doesnt seem to work fine with it
        [ManyToOne(Column = "subcontractorid", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public SubContractor SubContractor { get; set; }

        [Property]
        public DateTime? SendTime { get; set; }

        [Property]
        public string Status { get; set; }

        [Property]
        public DateTime? ExpirationDate { get; set; }

        [Property]
        public string PoNumber { get; set; }

        [Property]
        public string ToNumber { get; set; }

        [Property]
        public string Email { get; set; }

        [Property]
        public string SiteName { get; set; }

        [Property]
        public string BillingEntity { get; set; }

        [Property]
        public string NotToExceedAmount { get; set; }

        [Property]
        public string RemainingFunds { get; set; }

        [Property]
        public string ScopeOfWork { get; set; }

        [Property]
        public string PlantContacts { get; set; }

        [Property]
        public string OtherInfo { get; set; }
    }
}
