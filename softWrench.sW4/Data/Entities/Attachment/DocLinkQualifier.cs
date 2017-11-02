using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Data.Entities.Attachment {

    [Class(Table = "SW_DOCLINK_QFR", Lazy = false)]
    public class DocLinkQualifier : IBaseEntity{

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        [Property]
        public string Qualifier { get; set; }
      

    }
}
