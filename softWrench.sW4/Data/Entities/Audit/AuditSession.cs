using NHibernate.Mapping.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.Entities.Audit {

    [Class(Table = "AUD_SESSION", Lazy = false)]
    public class AuditSession {
        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual int? UserId { get; set; }

        [Property]
        public virtual DateTime? StartDate { get; set; }

        [Property]
        public virtual DateTime? EndDate { get; set; }
    }
}
