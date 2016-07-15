using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {

    [Class(Table = "PREF_GENERICPROPERTIES", Lazy = false)]
    public class GenericProperties {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property(Column = "key")]
        public string Key {
            get; set;
        }

        [Property(Column = "value")]
        public string Value {
            get; set;
        }


    }
}
