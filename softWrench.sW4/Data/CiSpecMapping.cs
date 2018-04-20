using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Interfaces;

namespace softWrench.sW4.Data {

    [Class(Table = "sw_cimapping", Lazy = false)]
    public class CiSpecMapping : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual string ServiceITValue { get; set; }

        [Property]
        public virtual string MaximoValue { get; set; }


    }
}
