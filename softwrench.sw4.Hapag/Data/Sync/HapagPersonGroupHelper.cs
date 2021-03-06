﻿using Iesi.Collections;
using Iesi.Collections.Generic;
using log4net;
using softwrench.sw4.Hapag.Data.Init;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using System.Collections.Generic;
using System.Linq;
using c = softwrench.sw4.Hapag.Data.Sync.HapagPersonGroupConstants;

namespace softwrench.sw4.Hapag.Data.Sync {
    public class HapagPersonGroupHelper : ISingletonComponent {

        private readonly SWDBHibernateDAO _dao;

        private static readonly ILog Log = LogManager.GetLogger(typeof(HapagPersonGroupHelper));
        private static IDictionary<string, UserProfile> _hapagProfiles;
        private static IDictionary<string, Role> _hapagModules;
        private static IDictionary<RoleType, Role> _cachedDefaultRoles;

        public HapagPersonGroupHelper(SWDBHibernateDAO dao) {
            this._dao = dao;
        }

        public void Init() {
            if (_hapagProfiles == null) {
                _hapagProfiles = GetHapagProfiles();
            }
            if (_hapagModules == null) {
                _hapagModules = GetHapagModules();
            }
            if (_cachedDefaultRoles == null) {
                _cachedDefaultRoles = GetDefaultRoles();
            }

        }



        public bool AddHapagMatchingRolesAndProfiles(PersonGroup personGroup, User user) {
            var addedProfile = false;
            var addedRole = false;
            if (_hapagProfiles.ContainsKey(personGroup.Name)) {
                if (user.Profiles == null) {
                    user.Profiles = new HashedSet<UserProfile>();
                }
                addedProfile = user.Profiles.Add(_hapagProfiles[personGroup.Name]);
            } else if (_hapagModules.ContainsKey(personGroup.Name)) {
                if (user.CustomRoles == null) {
                    user.CustomRoles = new HashedSet<UserCustomRole>();
                }
                var hapagModule = _hapagModules[personGroup.Name];
                Log.DebugFormat("adding customrole {0} to user {1} from group {2}", hapagModule.Name, user.Id,
                    personGroup.Name);
                addedRole = user.CustomRoles.Add(new UserCustomRole { Role = hapagModule, UserId = user.Id });
            }
            return addedProfile || addedRole;
        }


        private IDictionary<RoleType, Role> GetDefaultRoles() {
            IDictionary<RoleType, Role> roles = new Dictionary<RoleType, Role>();
            roles.Add(RoleType.Defaulthome, FindRole(RoleType.Defaulthome));
            roles.Add(RoleType.Defaultnewsr, FindRole(RoleType.Defaultnewsr));
            roles.Add(RoleType.Defaultsrgrid, FindRole(RoleType.Defaultsrgrid));
            roles.Add(RoleType.Defaultssrsearch, FindRole(RoleType.Defaultssrsearch));
            roles.Add(RoleType.IncidentDetailsReport, FindRole(RoleType.IncidentDetailsReport));
            roles.Add(RoleType.Ci, FindRole(RoleType.Ci));
            return roles;
        }

        private IDictionary<string, Role> GetHapagModules() {
            IDictionary<string, Role> resultDict = new Dictionary<string, Role>();
            resultDict[c.ActrlRam] = FindRole(FunctionalRole.AssetRamControl);
            resultDict[c.Actrl] = FindRole(FunctionalRole.AssetControl);
            resultDict[c.XITC] = FindRole(FunctionalRole.XItc);
            resultDict[c.Purchase] = FindRole(FunctionalRole.Purchase);
            resultDict[c.Ad] = FindRole(FunctionalRole.Ad);
            resultDict[c.Tom] = FindRole(FunctionalRole.Tom);
            resultDict[c.Itom] = FindRole(FunctionalRole.Itom);
            resultDict[c.Change] = FindRole(FunctionalRole.Change);
            resultDict[c.Offering] = FindRole(FunctionalRole.Offering);
            resultDict[c.SSO] = FindRole(FunctionalRole.Sso);
            resultDict[c.Tui] = FindRole(FunctionalRole.Tui);
            return resultDict;
        }

        private IDictionary<string, UserProfile> GetHapagProfiles() {
            IDictionary<string, UserProfile> resultDict = new Dictionary<string, UserProfile>();
            //            resultDict[HapagPersonGroupConstants.HEu] = FindUserProfile(ProfileType.EndUser);
            resultDict[c.HITC] = FindUserProfile(ProfileType.Itc);
            return resultDict;
        }

        private UserProfile FindUserProfile(ProfileType profileType) {
            return _dao.FindSingleByQuery<UserProfile>(UserProfile.UserProfileByName, profileType.GetName());
        }

        private Role FindRole(FunctionalRole roleType) {
            var role = _dao.FindSingleByQuery<Role>(Role.RoleByName, roleType.GetName());
            if (role == null) {
                Log.WarnFormat("role {0} not found for group", roleType.GetName());
            }
            return role;
        }

        private Role FindRole(RoleType roleType) {
            var role = _dao.FindSingleByQuery<Role>(Role.RoleByName, roleType.GetName());
            if (role == null) {
                Log.WarnFormat("role {0} not found for group", roleType.GetName());
            }
            return role;
        }


        public InMemoryUser RemoveOrphanEntities(InMemoryUser user) {
            Iesi.Collections.Generic.ISet<Role> rolesToRemove = new HashedSet<Role>();

            Iesi.Collections.Generic.ISet<UserCustomRole> customRolesToDelete = new HashedSet<UserCustomRole>();
            Iesi.Collections.Generic.ISet<UserProfile> profilesToremove = new HashedSet<UserProfile>();
            foreach (var role in _hapagModules.Keys) {
                if (user.Roles.Contains(_hapagModules[role]) &&
                    !user.PersonGroups.Any(r => r.PersonGroup.Name.Equals(role))) {
                    rolesToRemove.Add(_hapagModules[role]);
                }
            }

            foreach (var profile in _hapagProfiles.Keys) {
                if (user.Profiles.Contains(_hapagProfiles[profile]) &&
                    !user.PersonGroups.Any(r => r.PersonGroup.Name.Equals(profile))) {
                    profilesToremove.Add(_hapagProfiles[profile]);
                }
            }



            if (!rolesToRemove.Any() && !profilesToremove.Any()) {
                return user;
            }

            var dbUser = user.DBUser;
            if (profilesToremove.Any()) {
                foreach (var userProfile in profilesToremove) {
                    dbUser.Profiles.Remove(userProfile);
                }
                _dao.Save(dbUser);
            }

            if (rolesToRemove.Any()) {
                foreach (var role in rolesToRemove) {
                    var customRole = dbUser.CustomRoles.FirstOrDefault(cr => cr.Role.Equals(role));
                    if (customRole == null) {
                        continue;
                    }
                    customRolesToDelete.Add(customRole);
                    dbUser.CustomRoles.Remove(customRole);
                }
                _dao.DeleteCollection(customRolesToDelete);
            }

            return new InMemoryUser(dbUser, dbUser.Profiles, user.TimezoneOffset);
        }

        private bool isEndUserOrITC(InMemoryUser user) {
            if (user.HasProfile(ProfileType.Itc)) {
                return true;
            }
            return user.PersonGroups.Any(f => f.PersonGroup.Name.EqualsAny(c.HEu, c.HExternalUser, c.HITC));
        }

        /// <summary>
        /// if user is external, then inactivate all %IFU% roles, and if internal, inactivate all %EFU% roles.
        /// 
        /// This is double check to avoid wrong scenarios on maximo side
        /// 
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public InMemoryUser HandleExternalUser(InMemoryUser user) {
            var isExternalUser =
                user.PersonGroups.Any(f => f.PersonGroup.Name.Equals(c.HExternalUser));
            var prefixToInactivate = isExternalUser ? c.InternalRolesPrefix : c.ExternalRolesPrefix;
            if (!isEndUserOrITC(user)) {
                //not external, nor enduser(with roles), nor itc... user is ordinary enduser!
                prefixToInactivate = c.BaseHapagPrefixNoWildcard;
            }

            foreach (var role in user.Roles) {
                var module = _hapagModules.FirstOrDefault(m => m.Value.Name.Equals(role.Name));
                if (module.Key != null && module.Key.StartsWith(prefixToInactivate)) {
                    Log.WarnFormat("marking role {0} as inactive to user {1}", role.Name, user.Login);
                    role.Active = false;
                }
            }

            if (user.IsInRole(FunctionalRole.Offering.ToString()) && !(user.IsInRole(FunctionalRole.Tom.ToString()) || user.IsInRole(FunctionalRole.Itom.ToString()))) {
                //offering require either tom or itom, as due to thomas email comments
                user.Roles.First(r => r.Name.EqualsIc(FunctionalRole.Offering.ToString())).Active = false;
            }




            return user;

        }

        /// <summary>
        /// If the user is not an external user, i.e TUI or SSO, we need to add default roles to it, so that the "enduser" actions become visible; 
        /// otherwise, the sso/tui roles will take control over it
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public InMemoryUser HandleSsotuiModulesMerge(InMemoryUser user) {
            var isExternalUser = user.PersonGroups.Any(f => f.PersonGroup.Name.Equals(c.HExternalUser));
            var isSSO = isExternalUser && user.PersonGroups.Any(f => f.PersonGroup.Name.Equals(c.SSO));
            var isTui = isExternalUser && user.PersonGroups.Any(f => f.PersonGroup.Name.Equals(c.Tui));


            var dbUser = user.DBUser;
            if (dbUser.CustomRoles == null) {
                dbUser.CustomRoles = new HashedSet<UserCustomRole>();
            }
            if (!isSSO && !isTui) {
                //if not a tui nor sso, we need to add the default items on the menu (enduser...)
                AddCustomRole(user, RoleType.Defaulthome);
                AddCustomRole(user, RoleType.Defaultnewsr);
                AddCustomRole(user, RoleType.Defaultsrgrid);
                AddCustomRole(user, RoleType.Defaultssrsearch);
            } else {
                //otherwise, these items cannot be seem, just the module ones
                RemoveCustomRole(user, RoleType.Defaulthome);
                RemoveCustomRole(user, RoleType.Defaultnewsr);
                RemoveCustomRole(user, RoleType.Defaultsrgrid);
                RemoveCustomRole(user, RoleType.Defaultssrsearch);

                //remove it due to legacy data on swdb
                RemoveCustomRole(user, RoleType.IncidentDetailsReport);
            }
            return user;
        }

        public InMemoryUser HandleTomItomModulesMerge(InMemoryUser user) {
            var isTomOrITom = user.PersonGroups.Any(f => f.PersonGroup.Name.EqualsAny(c.Tom, c.Itom));
            var isEUOrITC = user.PersonGroups.Any(f => f.PersonGroup.Name.EqualsAny(c.HEu, c.HITC));
            if (isEUOrITC && isTomOrITom) {
                AddCustomRole(user, RoleType.Ci);
            } else {
                RemoveCustomRole(user, RoleType.Ci);
            }

            return user;
        }

        private static void AddCustomRole(InMemoryUser user, RoleType type) {
            var dbUser = user.DBUser;
            dbUser.CustomRoles.Add(new UserCustomRole {
                Role = _cachedDefaultRoles[type],
                UserId = dbUser.Id
            });
            user.Roles.Add(_cachedDefaultRoles[type]);
        }

        private static void RemoveCustomRole(InMemoryUser user, RoleType type) {
            var dbUser = user.DBUser;
            dbUser.CustomRoles.Remove(new UserCustomRole {
                Role = _cachedDefaultRoles[type],
                UserId = dbUser.Id
            });
            user.Roles.Remove(_cachedDefaultRoles[type]);
        }

        public bool HasMissingRoles(InMemoryUser user) {
            foreach (var personGroup in user.PersonGroups) {
                if (!_hapagModules.ContainsKey(personGroup.GroupName)) {
                    continue;
                }
                var module = _hapagModules[personGroup.GroupName];
                if (!user.Roles.Contains(module)) {
                    return true;
                }
            }
            return false;
        }


    }
}
