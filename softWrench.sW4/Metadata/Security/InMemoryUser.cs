﻿using cts.commons.portable.Util;
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
using softWrench.sW4.Security.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
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
        private readonly int? _timezoneOffset;
        private readonly UserPreferences _userPreferences;
        private readonly IList<Role> _roles;
        private readonly ICollection<UserProfile> _profiles;
        private readonly Iesi.Collections.Generic.ISet<PersonGroupAssociation> _personGroups;
        private readonly IList<DataConstraint> _dataConstraints;
        private readonly IDictionary<ClientPlatform, MenuDefinition> _cachedMenu = new ConcurrentDictionary<ClientPlatform, MenuDefinition>();
        private readonly IDictionary<ClientPlatform, IDictionary<string, CommandBarDefinition>> _cachedBars = new ConcurrentDictionary<ClientPlatform, IDictionary<string, CommandBarDefinition>>();
        private IDictionary<string, object> _genericproperties = new Dictionary<string, object>();

        private const string BlankUser = "menu is blank for user {0} review his security configuration";
        private const string MenuNotFound = "menu not found for platform {0}. ";

        private static readonly ILog Log = LogManager.GetLogger(typeof(InMemoryUserExtensions));

        internal static InMemoryUser TestInstance(string login = null) {
            return new InMemoryUser { _login = login };
        }

        private InMemoryUser() {
            _roles = new List<Role>();
        }

        public InMemoryUser(User dbUser, IEnumerable<UserProfile> initializedProfiles, UserPreferences userPreferences, int? timezoneOffset) {
            DBUser = dbUser;
            _login = dbUser.UserName;
            SiteId = dbUser.SiteId;
            _firstName = dbUser.FirstName;
            _lastName = dbUser.LastName;
            _email = dbUser.Email;
            _orgId = dbUser.OrgId;
            _storeloc = dbUser.Storeloc;
            _department = dbUser.Department;
            _phone = dbUser.Phone;
            _language = dbUser.Language;
            _dbId = dbUser.Id;
            _timezoneOffset = timezoneOffset;
            _maximoPersonId = dbUser.MaximoPersonId;
            _personGroups = (dbUser.PersonGroups ?? new HashedSet<PersonGroupAssociation>());
            var userProfiles = initializedProfiles as UserProfile[] ?? initializedProfiles.ToArray();
            _profiles = userProfiles;
            var roles = new List<Role>();
            var dataConstraints = new List<DataConstraint>();
            foreach (var profile in userProfiles) {
                roles.AddRange(profile.Roles);
                dataConstraints.AddRange(profile.DataConstraints);
            }
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
            _userPreferences = userPreferences;
        }

        public string Login {
            get { return _login; }
            set { _login = value; }
        }

        public string SiteId { get; set; }

        public string FirstName {
            get { return _firstName; }
        }

        public string LastName {
            get { return _lastName; }
        }

        public string Email {
            get { return _email; }
        }

        public string OrgId {
            get { return _orgId; }
        }

        public Iesi.Collections.Generic.ISet<PersonGroupAssociation> PersonGroups {
            get { return _personGroups; }
        }

        public string Department {
            get { return _department; }
        }

        public string Phone {
            get { return _phone; }
        }

        public string Language {
            get { return _language; }
        }

        public string Storeloc {
            get { return _storeloc; }
        }

        public string MaximoPersonId {
            get { return _maximoPersonId; }
        }

        public UserPreferences UserPreferences {
            get { return _userPreferences; }
        }

        /// <summary>
        /// Time difference between UTC time and the user local time, in minutes
        /// (UTCTime - ClientTime)
        /// </summary>
        public int? TimezoneOffset {
            get { return _timezoneOffset; }
        }

        public IList<Role> Roles {
            get { return _roles; }
        }

        public int? DBId {
            get { return _dbId; }
        }

        [JsonIgnore]
        public User DBUser { get; set; }


        public IList<DataConstraint> DataConstraints {
            get { return _dataConstraints; }
        }

        public MenuDefinition Menu(ClientPlatform platform) {
            if (_cachedMenu.ContainsKey(platform)) {
                return _cachedMenu[platform];
            }

            var unsecureMenu = MetadataProvider.Menu(platform);
            if (unsecureMenu == null) {
                Log.Warn(String.Format(MenuNotFound, platform));
                return null;
            }

            var secureLeafs = new List<MenuBaseDefinition>();
            if (unsecureMenu.Leafs != null) {
                foreach (var leaf in unsecureMenu.Leafs) {
                    if (!Login.Equals("swadmin") && leaf.Role != null &&
                        (Roles == null || !Roles.Any(r => r.Active && r.Name == leaf.Role))) {
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

        public ICollection<UserProfile> Profiles {
            get { return _profiles; }
        }

        public bool IsInRole(string role) {
            if (IsSwAdmin() && ApplicationConfiguration.IsLocal()) {
                return true;
            }

            return _roles.Any(r => r.Name.EqualsIc(role));
        }
        [JsonIgnore]
        public IIdentity Identity { get; private set; }

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
            get { return DBId; }
        }

        public IDictionary<string, object> Genericproperties {
            get { return _genericproperties; }
            set { _genericproperties = value; }
        }

        public string GetPersonGroupsForQuery() {
            var personGroups = PersonGroups.Select(f => f.PersonGroup.Name).ToArray();
            var strPersonGroups = String.Join("','", personGroups);

            strPersonGroups = "'" + strPersonGroups + "'";
            return strPersonGroups;
        }

        public string FullName {
            get { return FirstName + " " + LastName; }
        }

        public bool IsSwAdmin() {
            return Login.Equals("swadmin") || IsInRole(Role.SysAdmin);
        }

        public IDictionary<string, CommandBarDefinition> SecuredBars(ClientPlatform platform, IDictionary<string, CommandBarDefinition> commandBars) {
            if (_cachedBars.ContainsKey(platform)) {
                return _cachedBars[platform];
            }
            var commandBarDefinitions = ApplicationCommandUtils.SecuredBars(this, commandBars);
            _cachedBars[platform] = commandBarDefinitions;
            return commandBarDefinitions;
        }
    }
}
