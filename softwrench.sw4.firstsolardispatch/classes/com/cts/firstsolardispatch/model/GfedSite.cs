using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model {

    [Class(Table = "GFED_SITE", Lazy = false)]
    public class GfedSite : IBaseEntity {
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

    }
}
