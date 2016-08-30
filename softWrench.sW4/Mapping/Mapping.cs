using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Mapping {


    [Class(Table = "SW_MAPPING", Lazy = false)]
    public class Mapping {

        public const string ByKey = "from Mapping where Key = ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public string Key {
            get; set;
        }

        [Property]
        public string OriginValue {
            get; set;
        }

        [Property]
        public string DestinationValue {
            get; set;
        }

        internal static Mapping TestValue(string key, string originValue, string destinationValue, int? id =null) {
            return new Mapping() {
                Key = key,
                OriginValue = originValue,
                DestinationValue = destinationValue,
                Id = id
            };
        }

    }
}
