using Iesi.Collections.Generic;
using log4net;
using Newtonsoft.Json;
using softwrench.sw4.api.classes.user;
using softWrench.sW4.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using softwrench.sw4.user.classes.entities;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Preferences;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Security {
    public class InMemoryUser : ISWUser {
        private MergedUserProfile _mergedUserProfile;
        internal IDictionary<ClientPlatform, MenuDefinition> CachedMenu = new ConcurrentDictionary<ClientPlatform, MenuDefinition>();
        private IDictionary<ClientPlatform, IDictionary<string, CommandBarDefinition>> _cachedBars = new ConcurrentDictionary<ClientPlatform, IDictionary<string, CommandBarDefinition>>();

        private static readonly ILog Log = LogManager.GetLogger(typeof(InMemoryUserExtensions));

        internal static InMemoryUser TestInstance(string login = null) {
            return new InMemoryUser { Login = login, TimezoneOffset = 420 };
        }

        private InMemoryUser() {
            Roles = new List<Role>();
            PersonGroups = new LinkedHashSet<PersonGroupAssociation>();
            Profiles = new List<UserProfile>();
            DataConstraints = new List<DataConstraint>();
        }

        public InMemoryUser(User dbUser, IEnumerable<UserProfile> initializedProfiles, [CanBeNull]GridPreferences gridPreferences, [CanBeNull]UserPreferences userPreferences, int? timezoneOffset, MergedUserProfile mergedProfile) {
            DBUser = dbUser;
            Login = dbUser.UserName;
            SiteId = dbUser.Person.SiteId ?? dbUser.SiteId;
            FirstName = dbUser.Person.FirstName ?? dbUser.FirstName;
            LastName = dbUser.Person.LastName ?? dbUser.LastName;
            Email = dbUser.Person.Email ?? dbUser.Email;
            OrgId = dbUser.Person.OrgId ?? dbUser.OrgId;
            Storeloc = dbUser.Person.Storeloc;
            Department = dbUser.Person.Department;
            Phone = dbUser.Person.Phone;
            Language = dbUser.Person.Language;
            DBId = dbUser.Id;
            TimezoneOffset = timezoneOffset;
            MaximoPersonId = dbUser.MaximoPersonId;
            PersonGroups = (dbUser.PersonGroups ?? new LinkedHashSet<PersonGroupAssociation>());
            _mergedUserProfile = mergedProfile;
            ChangePassword = dbUser.ChangePassword ?? false;
            PasswordExpirationTime = dbUser.PasswordExpirationTime;
            var userProfiles = initializedProfiles as UserProfile[] ?? initializedProfiles.ToArray();
            Profiles = userProfiles;
            var roles = new List<Role>(mergedProfile.Roles);
            var dataConstraints = new List<DataConstraint>();
            Locked = dbUser.Locked ?? false;

            if (dbUser.CustomRoles != null) {
                foreach (var role in dbUser.CustomRoles) {
                    if (role.Exclusion) {
                        roles.Remove(role.Role);
                    } else {
                        roles.Add(role.Role);
                    }
                }
            }

            if (dbUser.PersonGroups != null) {
                foreach (var personGroup in dbUser.PersonGroups) {
                    PersonGroups.Add(personGroup);
                }
            }
            if (dbUser.CustomConstraints != null) {
                foreach (var constraint in dbUser.CustomConstraints) {
                    if (constraint.Exclusion) {
                        dataConstraints.Remove(constraint.Constraint);
                    } else {
                        dataConstraints.Add(constraint.Constraint);
                    }
                }
            }
            Roles = roles;
            DataConstraints = dataConstraints;
            Identity = new GenericIdentity(Login);
            GridPreferences = gridPreferences;
            UserPreferences = userPreferences;
            HandleUserPreferences(userPreferences);
            _mergedUserProfile = mergedProfile;
            Active = dbUser.IsActive;
        }

        private void HandleUserPreferences(UserPreferences userPreferences) {
            if (userPreferences != null) {
                Signature = userPreferences.Signature;
                if (UserPreferences.GenericProperties != null) {
                    foreach (var genericProperty in UserPreferences.GenericProperties) {
                        Genericproperties.Add(genericProperty.Key, genericProperty.Convert());
                    }
                }
            }
        }

        private InMemoryUser(string mock) : this() {
            Login = mock;
            FirstName = mock;
            LastName = mock;
            MaximoPersonId = mock;
            SiteId = mock;
            DBId = int.MinValue;
            TimezoneOffset = Convert.ToInt32(TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMinutes);
        }

        public static InMemoryUser NewAnonymousInstance(bool active = true, bool locked=false) {
            return new InMemoryUser("anonymous") { Active = active, Locked = locked};
        }

        public Boolean IsAnonymous() {
            return Login == "anonymous" && DBId.HasValue && DBId.Value == int.MinValue;
        }

        public string Login { get; set; }

        public string SiteId {
            get; set;
        }

        public string FirstName { get; }

        public string LastName { get; }

        public string Email { get; }

        public string OrgId { get; set; }

        public ISet<PersonGroupAssociation> PersonGroups { get; }

        public string Department { get; }

        public string Phone { get; }

        public string Language { get; }

        public string Storeloc { get; }

        public string MaximoPersonId { get; }

        public string Signature { get; private set; }

        public GridPreferences GridPreferences { get; }

        public UserPreferences UserPreferences { get; }

        public bool? Active { get; private set; }

        public bool Locked {get; private set; }

        /// <summary>
        /// Time difference between UTC time and the user local time, in minutes
        /// (UTCTime - ClientTime)
        /// </summary>
        public int? TimezoneOffset { get; set; }

        public IList<Role> Roles { get; }

        public int? DBId { get; }

        public bool ChangePassword { get; }

        public DateTime? PasswordExpirationTime {get;}

        [JsonIgnore]
        public User DBUser {
            get; set;
        }

        [JsonIgnore]
        [NotNull]
        //used for security mech
        public MergedUserProfile MergedUserProfile {
            get {
                if (_mergedUserProfile == null) {
                    _mergedUserProfile = new MergedUserProfile();
                }
                return _mergedUserProfile;
            }
        }


        public IList<DataConstraint> DataConstraints { get; }


        [NotNull]
        public ICollection<UserProfile> Profiles { get; }

        public bool IsAllowedInApp(string applicationName) {
            if (IsInRolInternal(applicationName)) {
                return true;
            }
            var applicationPermission = MergedUserProfile.Permissions.FirstOrDefault(a => a.ApplicationName.Equals(applicationName));
            return applicationPermission != null && !applicationPermission.HasNoPermissions;
        }


        public bool IsInRole(string role) {
            return IsInRolInternal(role);
        }

        public bool IsInRolInternal(string role, bool checkSwAdmin = true) {
            if (checkSwAdmin && IsSwAdmin()) {
                return true;
            }
            if (string.IsNullOrEmpty(role)) {
                return true;
            }

            return Roles.Any(r => r.Name.EqualsIc(role));
        }

        public bool IsInGroup(string groupName) {
            return PersonGroups.Any(g => g.GroupName.Equals(groupName));
        }

        [JsonIgnore]
        public IIdentity Identity {
            get; private set;
        }

        public bool IsInProfile(string profileName) {
            if (IsSwAdmin() && ApplicationConfiguration.IsLocal()) {
                return true;
            }
            return Profiles != null && Profiles.Any(p => p.Name.EqualsIc(profileName));
        }

        public IEnumerable<int?> ProfileIds {
            get {
                if (Profiles == null) {
                    return new List<int?>();
                }
                return Profiles.Select(p => p.Id);
            }
        }

        public int? UserId {
            get {
                return DBId;
            }
        }

        public IDictionary<string, object> Genericproperties { get; set; } = new Dictionary<string, object>();

        public IDictionary<string, object> GenericSyncProperties {
            get {
                var allProperties = Genericproperties;
                var syncProperties = allProperties == null
                    ? new Dictionary<string, object>()
                    : allProperties.Where(p => p.Key!=null && p.Key.StartsWith("sync.")).ToDictionary(p => p.Key, p => p.Value);
                syncProperties.Add("siteid", SiteId);
                syncProperties.Add("orgid", OrgId);
                if (Genericproperties.ContainsKey("laborcode")){
                    syncProperties.Add("laborcode", Genericproperties["laborcode"]);
                }
                return syncProperties;
            }
        }

        public string GetPersonGroupsForQuery() {
            var personGroups = PersonGroups.Select(f => f.PersonGroup.Name).ToArray();
            var strPersonGroups = String.Join("','", personGroups);

            strPersonGroups = "'" + strPersonGroups + "'";
            return strPersonGroups;
        }

        public string FullName {
            get {
                return FirstName + " " + LastName;
            }
        }

        public int SessionAuditId {
            get;  set;
        }

        public bool IsSwAdmin() {
            return Login.Equals("swadmin") || (IsInRolInternal(Role.SysAdmin, false) && IsInRolInternal(Role.ClientAdmin, false));
        }



        public IDictionary<string, CommandBarDefinition> SecuredBars(ClientPlatform platform, IDictionary<string, CommandBarDefinition> commandBars, ApplicationSchemaDefinition currentSchema = null) {
            if (currentSchema == null && _cachedBars.ContainsKey(platform)) {
                return _cachedBars[platform];
            }
            var commandBarDefinitions = ApplicationCommandUtils.SecuredBars(this, commandBars);
            _cachedBars[platform] = commandBarDefinitions;
            return commandBarDefinitions;
        }

        public void ClearMenu() {
            CachedMenu = new ConcurrentDictionary<ClientPlatform, MenuDefinition>();
            Genericproperties.Remove("menumanagerscached");
        }

        public void ClearBars() {
            _cachedBars = new ConcurrentDictionary<ClientPlatform, IDictionary<string, CommandBarDefinition>>();
        }

        [CanBeNull]
        public object GetProperty(string key) {
            if (Genericproperties.ContainsKey(key)) {
                return Genericproperties[key];
            }
            return null;
        }


        public void AddGenericProperties(string key, object value) {
            if (Genericproperties.ContainsKey(key)) {
                Genericproperties.Remove(key);
            }
            Genericproperties.Add(key, value);
        }
    }
}
