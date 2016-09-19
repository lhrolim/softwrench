using cts.commons.portable.Util;
using Iesi.Collections.Generic;
using log4net;
using softwrench.sw4.Hapag.Data.Init;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata.Security;
using cts.commons.simpleinjector;
using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.user.classes.entities;
using c = softwrench.sw4.Hapag.Data.Sync.HapagPersonGroupConstants;

namespace softwrench.sw4.Hapag.Data.Sync {
    public class HapagPersonGroupHelper : ISingletonComponent {

        private readonly SWDBHibernateDAO _dao;

        private static readonly ILog Log = LogManager.GetLogger(typeof(HapagPersonGroupHelper));
        private static IDictionary<string, UserProfile> _hapagProfiles;
        private static IDictionary<string, Role> _hapagModules;

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
        }

        public bool AddHapagMatchingRolesAndProfiles(PersonGroup personGroup, User user) {
            var addedProfile = false;
            var addedRole = false;
            if (_hapagProfiles.ContainsKey(personGroup.Name)) {
                if (user.Profiles == null) {
                    user.Profiles = new LinkedHashSet<UserProfile>();
                }
                addedProfile = user.Profiles.Add(_hapagProfiles[personGroup.Name]);
            } else if (_hapagModules.ContainsKey(personGroup.Name)) {
                if (user.CustomRoles == null) {
                    user.CustomRoles = new LinkedHashSet<UserCustomRole>();
                }
                var hapagModule = _hapagModules[personGroup.Name];
                Log.DebugFormat("adding customrole {0} to user {1} from group {2}", hapagModule.Name, user.Id,
                    personGroup.Name);
                addedRole = user.CustomRoles.Add(new UserCustomRole { Role = hapagModule, UserId = user.Id });
            }
            return addedProfile || addedRole;
        }


        private IDictionary<string, Role> GetHapagModules() {
            IDictionary<string, Role> resultDict = new Dictionary<string, Role>();
            resultDict[HapagPersonGroupConstants.ActrlRam] = FindRole(FunctionalRole.AssetRamControl);
            resultDict[HapagPersonGroupConstants.Actrl] = FindRole(FunctionalRole.AssetControl);
            resultDict[HapagPersonGroupConstants.XITC] = FindRole(FunctionalRole.XItc);
            resultDict[HapagPersonGroupConstants.Purchase] = FindRole(FunctionalRole.Purchase);
            resultDict[HapagPersonGroupConstants.Ad] = FindRole(FunctionalRole.Ad);
            resultDict[HapagPersonGroupConstants.Tom] = FindRole(FunctionalRole.Tom);
            resultDict[HapagPersonGroupConstants.Itom] = FindRole(FunctionalRole.Itom);
            resultDict[HapagPersonGroupConstants.Change] = FindRole(FunctionalRole.Change);
            resultDict[HapagPersonGroupConstants.SSO] = FindRole(FunctionalRole.Sso);
            resultDict[HapagPersonGroupConstants.Tui] = FindRole(FunctionalRole.Tui);
            return resultDict;
        }

        private IDictionary<string, UserProfile> GetHapagProfiles() {
            IDictionary<string, UserProfile> resultDict = new Dictionary<string, UserProfile>();
            //            resultDict[HapagPersonGroupConstants.HEu] = FindUserProfile(ProfileType.EndUser);
            resultDict[HapagPersonGroupConstants.HITC] = FindUserProfile(ProfileType.Itc);
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


        public InMemoryUser RemoveOrphanEntities(InMemoryUser user) {
            ISet<Role> rolesToRemove = new LinkedHashSet<Role>();

            ISet<UserCustomRole> customRolesToDelete = new LinkedHashSet<UserCustomRole>();
            ISet<UserProfile> profilesToremove = new LinkedHashSet<UserProfile>();
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

            return new InMemoryUser(dbUser, dbUser.Profiles,user.GridPreferences, user.UserPreferences, user.TimezoneOffset, user.MergedUserProfile);
        }

        private bool isEndUserOrITC(InMemoryUser user) {
            if (user.HasProfile(ProfileType.Itc)) {
                return true;
            }
            return user.PersonGroups.Any(f => f.PersonGroup.Name.EqualsAny(c.HEu, c.HExternalUser,c.HITC));
        }

        /// <summary>
        /// if user is external, then inactivate all %IFU% roles, and if external, inactivate all %EFU% roles.
        /// 
        /// This is double check to avoid wrong scenarios on maximo side
        /// 
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public InMemoryUser HandleExternalUser(InMemoryUser user) {
            var isExternalUser =
                user.PersonGroups.Any(f => f.PersonGroup.Name.Equals(HapagPersonGroupConstants.HExternalUser));
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
            return user;

        }
    }
}
