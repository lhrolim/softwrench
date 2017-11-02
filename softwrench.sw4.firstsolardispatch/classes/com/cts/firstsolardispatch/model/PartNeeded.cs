using System;
using cts.commons.persistence;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model {

    [Class(Table = "DISP_PART_NEEDED", Lazy = false)]
    public class PartNeeded : IBaseEntity {
        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public string PartNumber { get; set; }

        [Property]
        public string PartDescription { get; set; }

        [Property]
        public string DeliveryMethod { get; set; }

        [Property]
        public DateTime? ExpectedDate { get; set; }

        [Property]
        public string DeliveryLocation { get; set; }

        [JsonIgnore]
        [ManyToOne(NotNull = true, Column = "inverterid", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public Inverter Inverter { get; set; }
    }
}
