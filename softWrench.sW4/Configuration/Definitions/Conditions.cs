using softWrench.sW4.Security.Context;

namespace softWrench.sW4.Configuration.Definitions {
    public class Conditions {
        public const string AnyCondition = "#all";

        public string OrgId { get; set; }
        public string SiteId { get; set; }

        public string UserProfile { get; set; }

        public string User { get; set; }

        public string SchemaId { get; set; }

        public string Mode { get; set; }

        public string Platform { get; set; }

        public string Environment { get; set; }

        public string SerializeString() {
            return null;
        }

        public static Conditions DeSerializeString(string condition) {
            return null;
        }

        public bool IsMatchedBy(ContextHolder lookupContext) {
            return true;
        }

        protected bool Equals(Conditions other) {
            return string.Equals(OrgId, other.OrgId) && string.Equals(UserProfile, other.UserProfile)
                && string.Equals(SiteId, other.SiteId) && string.Equals(User, other.User)
                && string.Equals(SchemaId, other.SchemaId) && string.Equals(Mode, other.Mode)
                && string.Equals(Platform, other.Platform) && string.Equals(Environment, other.Environment);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Conditions)obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (OrgId != null ? OrgId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (UserProfile != null ? UserProfile.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SiteId != null ? SiteId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (User != null ? User.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SchemaId != null ? SchemaId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Mode != null ? Mode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Platform != null ? Platform.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Environment != null ? Environment.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString() {
            return string.Format("OrgId: {0}, SiteId: {1}, UserProfile: {2}, User: {3}, SchemaId: {4}, Mode: {5}, Platform: {6}, Environment: {7}", OrgId, SiteId, UserProfile, User, SchemaId, Mode, Platform, Environment);
        }
    }
}
