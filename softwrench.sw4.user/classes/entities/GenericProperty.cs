using System.Collections.Generic;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {

    [Class(Table = "PREF_GENERICPROPERTIES", Lazy = false)]
    public class GenericProperty {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property(Column = "key_")]
        public string Key {
            get; set;
        }

        [Property(Column = "value_")]
        public string Value {
            get; set;
        }

        [Property(Column = "type_")]
        public string Type {
            get; set;
        }

        [Newtonsoft.Json.JsonIgnore]
        [ManyToOne(Column = "preference_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public virtual UserPreferences UserPreferences {
            get; set;
        }


        public object Convert() {
            if (Type == null) {
                return Value;
            }
            if (Type.Equals("list")) {
                if (string.IsNullOrEmpty(Value)) {
                    return new List<string>();
                }
                return Value.Split(',');
            }
            return Value;
        }
    }
}
