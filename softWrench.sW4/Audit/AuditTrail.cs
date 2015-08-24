using System;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Audit {

    [Class(Table = "AUDI_TRAIL", Lazy = false)]
    public class AuditTrail {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual string Name { get; set; }

        [Property]
        public virtual DateTime BeginTime { get; set; }

        [Property]
        public virtual DateTime EndTime { get; set; }

    }
}
