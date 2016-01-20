using softWrench.sW4.Configuration.Definitions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sw4.user.classes.entities;

namespace softWrench.sW4.Security.Context {
    public class ContextHolder {

        public string OrgId {
            get; set;
        }
        public string SiteId {
            get; set;
        }

        public SortedSet<int?> UserProfiles {
            get; set;
        }

        public int? CurrentSelectedProfile {
            get; set;
        }

        public IEnumerable<UserProfile> AvailableProfilesForGrid {
            get; set;
        }

        public string User {
            get; set;
        }

        public string Mode {
            get; set;
        }

        public string Platform {
            get; set;
        }

        public string Environment {
            get; set;
        }

        public string Module {
            get; set;
        }

        public bool PrintMode {
            get; set;
        }

        public bool ScanMode {
            get; set;
        }

        public bool OfflineMode {
            get; set;
        }

        public bool MockMaximo {
            get; set;
        }

        private IDictionary<string, object> _parameters;

        public IDictionary<string, object> MetadataParameters {
            get {
                if (_parameters == null) {
                    _parameters = new Dictionary<string, object>();
                }
                return _parameters;
            }
            set {
                _parameters = value;
            }
        }

        public ApplicationLookupContext ApplicationLookupContext {
            get; set;
        }

        public ConditionMatch ProfileMatches(int? storedProfile) {
            if (storedProfile == null) {
                return ConditionMatch.Exact;
            }
            if (UserProfiles == null) {
                return ConditionMatch.No;
            }
            return UserProfiles.Contains(storedProfile) ? ConditionMatch.Exact : ConditionMatch.No;
        }



        public static ContextHolder DeSerializeString() {
            return null;
        }

        protected bool Equals(ContextHolder other) {

            var baseEqual = string.Equals(OrgId, other.OrgId) &&
                            string.Equals(SiteId, other.SiteId) &&
                            string.Equals(Mode, other.Mode) && string.Equals(Platform, other.Platform) &&
                            string.Equals(Environment, other.Environment) &&
                            string.Equals(User, other.User) &&
                            string.Equals(OfflineMode, other.OfflineMode) &&
                            string.Equals(MockMaximo, other.MockMaximo) &&
                            string.Equals(PrintMode, other.PrintMode) &&
                            string.Equals(ScanMode, other.ScanMode) &&
                            string.Equals(Module, other.Module);
            if (!baseEqual) {
                return false;
            }
            var profileEqual = false;
            if (UserProfiles == null) {
                profileEqual = other.UserProfiles == null;
            } else {
                if (other.UserProfiles != null) {
                    profileEqual = UserProfiles.SequenceEqual(other.UserProfiles);
                }
            }

            if (other.CurrentSelectedProfile != CurrentSelectedProfile) {
                profileEqual = false;
            }

            var appContextEqual = ApplicationLookupContext == null ? other.ApplicationLookupContext == null :
                ApplicationLookupContext.Equals(other.ApplicationLookupContext);
            return profileEqual && appContextEqual;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContextHolder)obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (OrgId != null ? OrgId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SiteId != null ? SiteId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (User != null ? User.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ApplicationLookupContext != null ? ApplicationLookupContext.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Mode != null ? Mode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Platform != null ? Platform.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PrintMode.GetHashCode());
                hashCode = (hashCode * 397) ^ (OfflineMode.GetHashCode());
                hashCode = (hashCode * 397) ^ (MockMaximo.GetHashCode());
                hashCode = (hashCode * 397) ^ (ScanMode.GetHashCode());
                hashCode = (hashCode * 397) ^ (Environment != null ? Environment.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Module != null ? Module.GetHashCode() : 0);
                return hashCode;
            }
        }

        public ConditionMatchResult MatchesCondition(Condition condition, ConditionMatchResult result) {
            if (condition == null) {
                return result.Append(ConditionMatch.GeneralMatch);
            }
            return condition.MatchesConditions(result, this);
        }

        public override string ToString() {
            var userProfiles = new StringBuilder();
            if (UserProfiles != null) {
                foreach (var profileId in UserProfiles) {
                    userProfiles.Append(profileId);
                    userProfiles.Append(",");
                }
                if (userProfiles.Length > 0) userProfiles.Remove(userProfiles.Length - 1, 1);
            }
            return string.Format("OrgId: {0}, SiteId: {1}, UserProfiles: {{{2}}}, User: {3}, Mode: {4}, Platform: {5}, Environment: {6}, Module: {7}, ApplicationLookupContext: {{{8}}}",
                OrgId, SiteId, userProfiles.ToString(), User, Mode, Platform, Environment, Module, ApplicationLookupContext);
        }

        public ContextHolder ShallowCopy() {
            return (ContextHolder)MemberwiseClone();
        }
    }
}
