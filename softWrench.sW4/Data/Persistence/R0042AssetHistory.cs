using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Interfaces;

namespace softWrench.sW4.Data.Persistence {


    [Class(Table = "HIST_ASSETR0042", Lazy = false)]
    public class R0042AssetHistory : IBaseEntity {


        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual string AssetId { get; set; }

        [Property]
        public virtual string Assetnum { get; set; }

        [Property]
        public virtual string ItcName { get; set; }

        [Property]
        public virtual string UserId { get; set; }

        [Property]
        public virtual string LocDescription { get; set; }

        [Property]
        public virtual string Department { get; set; }

        [Property]
        public virtual string Floor { get; set; }


        [Property]
        public virtual string Room { get; set; }

        [Property]
        public virtual string SerialNum { get; set; }

        [Property]
        public virtual string EosDate { get; set; }

        [Property]
        public virtual string Usage { get; set; }

        [Property]
        public virtual string Status { get; set; }

        [Property]
        public virtual string MacAddress { get; set; }

        [Property]
        public virtual DateTime? ChangeDate { get; set; }

        [Property]
        public virtual DateTime ExtractionDate { get; set; }


        public virtual long? Rowstamp { get; set; }



    }
}
