using System.Collections.Generic;
using cts.commons.persistence;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model {

    [Class(Table = "DISP_INVERTER", Lazy = false)]
    public class Inverter : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public string AssetNum {
            get; set;
        }

        [Property]
        public string Siteid {
            get; set;
        }

        [Property]
        public string Manufacturer {
            get; set;
        }

        [Property]
        public string Model {
            get; set;
        }

        [Property]
        public string ErrorCodes {
            get; set;
        }

        [Property]
        public string FailureClass {
            get; set;
        }

        [Property]
        public bool PartsRequired {
            get; set;
        }

        [Bag(0, Table = "DISP_PART_NEEDED", Cascade = "all", Lazy = CollectionLazy.False, Inverse = true, OrderBy = "PartNumber asc")]
        [Key(1, Column = "inverterid", NotNull = true)]
        [OneToMany(2, ClassType = typeof(PartNeeded))]
        public virtual IList<PartNeeded> PartsNeeded { get; set; }

        [JsonIgnore]
        [ManyToOne(NotNull = true, Column = "ticketid", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public DispatchTicket Ticket { get; set; }
    }
}
