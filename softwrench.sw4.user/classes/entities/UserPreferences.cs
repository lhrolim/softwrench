using cts.commons.persistence;
using Newtonsoft.Json.Linq;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {
    [Class(Table = "PREF_GENERAL", Lazy = false)]
    public sealed class UserPreferences : IBaseEntity {

        public const string PreferenesByUserId = "from UserPreferences where UserId = ?";
        public const string PreferenesByProfileId = "from UserPreferences where ProfileId = ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public int? UserId {
            get; set;
        }

        [Property]
        public int? ProfileId {
            get; set;
        }

        [Property]
        public string Signature{
            get; set;
        }

        public static UserPreferences FromJson(JToken jObject) {
            var preferences = new UserPreferences();
            preferences.Id = (int?)jObject["id"];
            preferences.UserId = (int?)jObject["userid"];
            preferences.ProfileId = (int?)jObject["profileid"];
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
    }
}
