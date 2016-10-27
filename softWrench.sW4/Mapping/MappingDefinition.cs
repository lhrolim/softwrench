using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Mapping {

    [Class(Table = "MAP_DEFINITION", Lazy = false)]
    public class MappingDefinition {

        public const string ByKey = "from MappingDefinition where Key_ = ?";


        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property(Column = "key_")]
        public string Key_ {
            get; set;
        }

        [Property]
        public string Description {
            get; set;
        }

        [Property (Column = "sourcealias")]
        public string SourceColumnAlias {
            get; set;
        }

        [Property(Column = "destinationalias")]
        public string DestinationColumnAlias {
            get; set;
        }





    }
}
