using Iesi.Collections.Generic;
using log4net;
using Newtonsoft.Json;
using softwrench.sw4.api.classes.user;
using softWrench.sW4.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softWrench.sW4.Metadata.Menu.Containers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Preferences;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Security {
    public class InMemoryUser : ISWUser {
        private readonly int? _dbId;
        private String _login;
        private readonly string _firstName;
        private readonly string _lastName;
        private readonly string _email;
        private readonly string _orgId;
        private readonly string _department;
        private readonly string _phone;
        private readonly string _language;
        private readonly string _maximoPersonId;
        private readonly string _storeloc;
        private readonly string _signature;
        private int? _timezoneOffset;
        private readonly GridPreferences _gridPreferences;
        private readonly UserPreferences _userPreferences;
        private readonly IList<Role> _roles;
        private readonly ICollection<UserProfile> _profiles;
        private readonly MergedUserProfile _mergedUserProfile;
        private readonly Iesi.Collections.Generic.ISet<PersonGroupAssociation> _personGroups;
        private readonly IList<DataConstraint> _dataConstraints;
        private IDictionary<ClientPlatform, MenuDefinition> _cachedMenu = new ConcurrentDictionary<ClientPlatform, MenuDefinition>();
        private IDictionary<ClientPlatform, IDictionary<string, CommandBarDefinition>> _cachedBars = new ConcurrentDictionary<ClientPlatform, IDictionary<string, CommandBarDefinition>>();
        private IDictionary<string, object> _genericproperties = new Dictionary<string, object>();

        private const string BlankUser = "menu is blank for user {0} review his security configuration";
        private const string MenuNotFound = "menu not found for platform {0}. ";

        private static readonly ILog Log = LogManager.GetLogger(typeof(InMemoryUserExtensions));

        internal static InMemoryUser TestInstance(string login = null) {
            return new InMemoryUser { _login = login, _timezoneOffset = 420 };
        }

        private InMemoryUser() {
            _roles = new List<Role>();
            _personGroups = new HashedSet<PersonGroupAssociation>();
            _profiles = new UserProfile[0];
            _dataConstraints = new List<DataConstraint>();
        }

        public InMemoryUser(User dbUser, IEnumerable<UserProfile> initializedProfiles, GridPreferences gridPreferences, UserPreferences userPreferences, int? timezoneOffset, MergedUserProfile mergedProfile) {
            DBUser = dbUser;
            _login = dbUser.UserName;
            SiteId = dbUser.Person.SiteId ?? dbUser.SiteId;
            _firstName = dbUser.Person.FirstName ?? dbUser.FirstName;
            _lastName = dbUser.Person.LastName ?? dbUser.LastName;
            _email = dbUser.Person.Email ?? dbUser.Email;
            _orgId = dbUser.Person.OrgId ?? dbUser.OrgId;
            _storeloc = dbUser.Person.Storeloc;
            _department = dbUser.Person.Department;
            _phone = dbUser.Person.Phone;
            _language = dbUser.Person.Language;
            _dbId = dbUser.Id;
            _timezoneOffset = timezoneOffset;
            _maximoPersonId = dbUser.MaximoPersonId;
            _personGroups = (dbUser.PersonGroups ?? new HashedSet<PersonGroupAssociation>());
            _mergedUserProfile = mergedProfile;
            var userProfiles = initializedProfiles as UserProfile[] ?? initializedProfiles.ToArray();
            _profiles = userProfiles;
            var roles = new List<Role>(mergedProfile.Roles);
            var dataConstraints = new List<DataConstraint>();

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
            _roles = roles;
            _dataConstraints = dataConstraints;
            Identity = new GenericIdentity(_login);
            _gridPreferences = gridPreferences;
            _userPreferences = userPreferences;
            _signature = userPreferences != null ? userPreferences.Signature : "";
        }

        private InMemoryUser(string mock) : this() {
            _login = mock;
            _firstName = mock;
            _lastName = mock;
            _maximoPersonId = mock;
            SiteId = mock;
            _dbId = int.MinValue;
            _timezoneOffset = Convert.ToInt32(TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMinutes);
        }

        public static InMemoryUser NewAnonymousInstance() {
            return new InMemoryUser("anonymous");
        }

        public Boolean IsAnonymous() {
            return _login == "anonymous" && _dbId.HasValue && _dbId.Value == int.MinValue;
        }

        public string Login {
            get {
                return _login;
            }
            set {
                _login = value;
            }
        }

        public string SiteId {
            get; set;
        }

        public string FirstName {
            get {
                return _firstName;
            }
        }

        public string LastName {
            get {
                return _lastName;
            }
        }

        public string Email {
            get {
                return _email;
            }
        }

        public string OrgId {
            get {
                return _orgId;
            }
        }

        public Iesi.Collections.Generic.ISet<PersonGroupAssociation> PersonGroups {
            get {
                return _personGroups;
            }
        }

        public string Department {
            get {
                return _department;
            }
        }

        public string Phone {
            get {
                return _phone;
            }
        }

        public string Language {
            get {
                return _language;
            }
        }

        public string Storeloc {
            get {
                return _storeloc;
            }
        }

        public string MaximoPersonId {
            get {
                return _maximoPersonId;
            }
        }

        public string Signature {
            get {
                return _signature;
            }
        }

        public GridPreferences GridPreferences {
            get {
                return _gridPreferences;
            }
        }

        public UserPreferences UserPreferences {
            get {
                return _userPreferences;
            }
        }

        /// <summary>
        /// Time difference between UTC time and the user local time, in minutes
        /// (UTCTime - ClientTime)
        /// </summary>
        public int? TimezoneOffset {
            get {
                return _timezoneOffset;
            }
        }

        public IList<Role> Roles {
            get {
                return _roles;
            }
        }

        public int? DBId {
            get {
                return _dbId;
            }
        }

        [JsonIgnore]
        public User DBUser {
            get; set;
        }

        [JsonIgnore]
        //used for security mech
        public MergedUserProfile MergedUserProfile {
            get {
                return _mergedUserProfile;
            }
        }


        public IList<DataConstraint> DataConstraints {
            get {
                return _dataConstraints;
            }
        }

        public MenuDefinition Menu(ClientPlatform platform, out Boolean fromCache) {
            if (_cachedMenu.ContainsKey(platform)) {
                fromCache = true;
                return _cachedMenu[platform];
            }
            fromCache = false;

            var unsecureMenu = MetadataProvider.Menu(platform);
            if (unsecureMenu == null) {
                Log.Warn(String.Format(MenuNotFound, platform));
                return null;
            }

            var secureLeafs = new List<MenuBaseDefinition>();
            if (unsecureMenu.Leafs != null) {
                foreach (var leaf in unsecureMenu.Leafs) {
                    if (!Login.Equals("swadmin") && leaf.Role != null &&
                        (Roles == null || leaf.IsRestrictedByRole(this))) {
                        Log.DebugFormat("ignoring leaf {0} for user {1} due to absence of role {2}", leaf.Id, Login, leaf.Role);
                        continue;
                    }
                    if (leaf is MenuContainerDefinition) {
                        var secured = ((MenuContainerDefinition)leaf).Secure(this);
                        if (secured != null) {
                            secureLeafs.Add(secured);
                        }
                    } else {
                        secureLeafs.Add(leaf);
                    }
                }
            }
            if (!secureLeafs.Any()) {
                Log.Warn(String.Format(BlankUser, Login));
            }
            var menuDefinition = new MenuDefinition(secureLeafs, unsecureMenu.MainMenuDisplacement.ToString(), unsecureMenu.ItemindexId);
            try {
                _cachedMenu.Add(platform, menuDefinition);
                // ReSharper disable once EmptyGeneralCatchClause
            } catch {
                //No op
            }
            return menuDefinition;
        }


        [NotNull]
        public ICollection<UserProfile> Profiles {
            get {
                return _profiles;
            }
        }

        public bool IsInRole(string role) {
            if (IsSwAdmin()) {
                return true;
            }
            if (string.IsNullOrEmpty(role)) {
                return true;
            }

            return _roles.Any(r => r.Name.EqualsIc(role));
        }

        public bool IsInGroup(string groupName) {
            return _personGroups.Any(g => g.GroupName.Equals(groupName));
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

        public IDictionary<string, object> Genericproperties {
            get {
                return _genericproperties;
            }
            set {
                _genericproperties = value;
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

        public bool IsSwAdmin() {
            return Login.Equals("swadmin");
        }

        public IDictionary<string, CommandBarDefinition> SecuredBars(ClientPlatform platform, IDictionary<string, CommandBarDefinition> commandBars) {
            if (_cachedBars.ContainsKey(platform)) {
                return _cachedBars[platform];
            }
            var commandBarDefinitions = ApplicationCommandUtils.SecuredBars(this, commandBars);
            _cachedBars[platform] = commandBarDefinitions;
            return commandBarDefinitions;
        }

        public void ClearMenu() {
            _cachedMenu = new ConcurrentDictionary<ClientPlatform, MenuDefinition>();
            Genericproperties.Remove("menumanagerscached");
        }

        [CanBeNull]
        public object GetProperty(string key) {
            if (Genericproperties.ContainsKey(key)) {
                return Genericproperties[key];
            }
            return null;
        }
    }
}
