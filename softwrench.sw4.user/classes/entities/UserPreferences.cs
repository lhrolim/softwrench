using System.Linq;
using cts.commons.persistence;
using Iesi.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {
    [Class(Table = "PREF_GENERALUSER", Lazy = false)]
    public sealed class UserPreferences : IBaseEntity {

        public const string PreferenesByUserId = "from UserPreferences where UserId = ?";
        public const string PreferenesByProfileId = "from UserPreferences where ProfileId = ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property(Column = "user_id")]
        public int? UserId {
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


        public static UserPreferences FromJson(JToken jObject) {
            var preferences = new UserPreferences();
            preferences.Id = (int?)jObject["id"];
            preferences.UserId = (int?)jObject["userid"];
            preferences.Signature = (string)jObject["signature"];
            return preferences;
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
