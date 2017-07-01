using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using Iesi.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {
    [Class(Table = "PREF_GENERALUSER", Lazy = false)]
    public sealed class UserPreferences : IBaseEntity {

        public const string PreferenesByUserId = "from UserPreferences where User.Id = ?";
        public const string PreferenesByProfileId = "from UserPreferences where ProfileId = ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [ManyToOne(Column = "user_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, NotNull =true)]
        [JsonIgnore]
        public User User {
            get; set;
        }

        [Property(Column = "signature")]
        public string Signature {
            get; set;
        }


        [Set(0, Table = "PREF_GENERICPROPERTIES", Lazy = CollectionLazy.False, Inverse = true, Cascade = "all-delete-orphan")]
        [Key(1, Column = "preference_id")]
        [OneToMany(2, ClassType = typeof(GenericProperty))]
        public ISet<GenericProperty> GenericProperties {
            get; set;
        }



        private bool Equals(UserPreferences other) {
            return Id == other.Id;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UserPreferences)obj);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public GenericProperty GetGenericProperty(string propKey) {
            if (GenericProperties == null) {
                return null;
            }
            return GenericProperties.FirstOrDefault(g => g.Key.Equals(propKey));
        }
    }
}
