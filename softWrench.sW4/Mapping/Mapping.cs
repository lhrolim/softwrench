using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Mapping {


    [Class(Table = "MAP_MAPPING", Lazy = false)]
    public class Mapping {

        public const string ByKey = "from Mapping where MappingDefinition.Key_ = ?";
        public const string ByMappingDefinition = "from Mapping where MappingDefinition.Id = ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
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

        [JsonIgnore]
        [ManyToOne(Column = "definition_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public MappingDefinition MappingDefinition {
            get; set;
        }

        internal static Mapping TestValue(string originValue, string destinationValue, int? id = null) {
            return new Mapping() {
                OriginValue = originValue,
                DestinationValue = destinationValue,
                Id = id
            };
        }

        public bool IsValid() {
            return !string.IsNullOrEmpty(OriginValue) && !string.IsNullOrEmpty(DestinationValue);
        }

    }

}
