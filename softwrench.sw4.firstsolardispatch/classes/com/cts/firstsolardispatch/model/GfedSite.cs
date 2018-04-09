using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model {

    [Class(Table = "GFED_SITE", Lazy = false)]
    public class GfedSite : IBaseEntity {
        public const string FromGFedId = "from GfedSite where GfedId = ?";

        public const string PrefixedQuery = "from GfedSite where facilityname = ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public long GfedId { get; set; }

        [Property]
        public string FacilityName { get; set; }

        [Property]
        public string FacilityTitle { get; set; }

        [Property]
        public string LocationPrefix { get; set; }

        [Property]
        public string SiteId { get; set; }

        [Property]
        public string OrgId { get; set; }

        [Property]
        public string Address { get; set; }

        [Property]
        public string City { get; set; }

        [Property]
        public string State { get; set; }

        [Property]
        public string PostalCode { get; set; }

        [Property]
        public string Country { get; set; }

        [Property]
        public string SingleLineAddress { get; set; }

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

        // tech - site manager
        [Property]
        public string PrimaryContact { get; set; }

        // tech - site manager
        [Property]
        public string PrimaryContactPhone { get; set; }

        // tech - site manager
        [Property]
        public string PrimaryContactEmail { get; set; }

        // tech - site manager
        [Property]
        public string PrimaryContactSmsEmail { get; set; }

        // RPM
        [Property]
        public string EscalationContact { get; set; }

        // RPM
        [Property]
        public string EscalationContactPhone { get; set; }

        // RPM
        [Property]
        public string EscalationContactEmail { get; set; }

        // RPM
        [Property]
        public string EscalationContactSmsEmail { get; set; }

        [Property]
        public decimal? GpsLatitude { get; set; }

        [Property]
        public decimal? GpsLongitude { get; set; }

        [Property]
        public string WherehouseAddress { get; set; }

    }
}
