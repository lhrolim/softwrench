using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sW4.audit.classes.Model {


    [Class(Table = "aud_query", Lazy = false)]
    public class AuditQuery : IBaseEntity{

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public string Query { get; set; }

        [Property]
        public string Qualifier { get; set; }

        [Property]
        public long? Ellapsedmillis { get; set; }

        [Property]
        public int? CountResult { get; set; }

        [Property]
        public DateTime? RegisterTime { get; set; }



    }
}
