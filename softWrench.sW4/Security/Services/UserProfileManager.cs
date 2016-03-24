using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.Util;
using Iesi.Collections.Generic;
using JetBrains.Annotations;
using NHibernate;
using log4net;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.SWDB;


namespace softWrench.sW4.Security.Services {
    public class UserProfileManager : ISingletonComponent {

        private static readonly ILog Log = LogManager.GetLogger(typeof(UserProfileManager));

        private static readonly IDictionary<int?, UserProfile> ProfileCache = new Dictionary<int?, UserProfile>();

        private static bool _cacheStarted;


        private readonly ISWDBHibernateDAO _dao;

        public UserProfileManager(ISWDBHibernateDAO dao) {
            _dao = dao;
        }

        public void ClearCache() {
            _cacheStarted = false;
            ProfileCache.Clear();
        }


        public UserProfile FindByName(string name) {
            if (!_cacheStarted) {
                FetchAllProfiles(true);
            }

            return ProfileCache.Values.FirstOrDefault(userProfile => userProfile.Name.EqualsIc(name));
        }


        [CanBeNull]
        public UserProfile FindById(int? id) {
            if (!_cacheStarted) {
                FetchAllProfiles(true);
            }
            if (ProfileCache.ContainsKey(id)) {
                return ProfileCache[id];
            }
            return null;
        }

        public ICollection<UserProfile> FetchAllProfiles(bool eager) {

            var before = Stopwatch.StartNew();
            if (_cacheStarted) {
                return ProfileCache.Values;
            }

            using (var session = SWDBHibernateDAO.GetInstance().GetSession()) {
                using (session.BeginTransaction()) {
                    var query = _dao.BuildQuery("from UserProfile", (object[])null, session);
                    var dbProfiles = query.List();
                    var profiles = new Dictionary<string, UserProfile>();
                    foreach (var profile in dbProfiles) {
                        var userProfile = (UserProfile)profile;
                        if (eager) {
                            NHibernateUtil.Initialize(userProfile.Roles);
                        }
                        profiles.Add(userProfile.Name, userProfile);
                        ProfileCache.Add(userProfile.Id, userProfile);
                    }
                    Log.Info(LoggingUtil.BaseDurationMessage("Profiles Loaded in {0}", before));
                    _cacheStarted = true;
                    return profiles.Values;
                }
            }

        }

        public UserProfile SaveUserProfile(UserProfile profile) {


            var isUpdate = profile.Id != null;
            if (!isUpdate) {
                var tempList = profile.ApplicationPermissions;
                profile.ApplicationPermissions = null;
                profile = _dao.Save(profile);
                profile.ApplicationPermissions = tempList;
            }

            if (profile.ApplicationPermissions != null) {
                foreach (var permission in profile.ApplicationPermissions) {
                    permission.Profile = profile;
                }
                _dao.BulkSave(profile.ApplicationPermissions);
            }
            profile = _dao.Save(profile);
            if (isUpdate) {
                if (ProfileCache.ContainsKey(profile.Id)) {
                    ProfileCache.Remove(profile.Id);
                }
                var storedProfile = ((SWDBHibernateDAO)_dao).EagerFindByPK<UserProfile>(typeof(UserProfile), profile.Id);
                ProfileCache.Add(profile.Id, storedProfile);
                return storedProfile;
            }
            ProfileCache.Add(profile.Id, profile);
            return profile;
        }

        public List<UserProfile> FindUserProfiles(User dbUser) {
            if (dbUser.Profiles == null) {
                return new List<UserProfile>();
            }
            return dbUser.Profiles.Select(profile => ProfileCache[profile.Id]).ToList();
        }



        //TODO: remove customUserRoles and customUSerCOnstraints which were exclusions from this profile ==> They don´t make sense anymore (tough,they are useless anyway)
        public void DeleteProfile(int? profileId) {
            using (ISession session = SWDBHibernateDAO.GetInstance().GetSession()) {
                using (session.BeginTransaction()) {
                    var profile = FindById(profileId);
                    if (profile == null) {
                        return;
                    }
                    _dao.DeleteCollection(profile.ApplicationPermissions);
                    _dao.ExecuteSql("delete from sw_user_userprofile where profile_id = {0}".Fmt(profile.Id));
                    _dao.Delete(profile);
                    if (ProfileCache.ContainsKey(profile.Id)) {
                        ProfileCache.Remove(profile.Id);
                    }
                }
            }
        }

        public CompositionFetchResult LoadAvailableFieldsAsCompositionData(ApplicationSchemaDefinition schema, string tab, int pageNumber) {
            var fields = schema.NonHiddenFieldsOfTab(tab).Where(f => !"true".Equals(f.RequiredExpression));
            var pageSize = 10;
            var applicationAttributeDisplayables = fields as IApplicationAttributeDisplayable[] ?? fields.ToArray();
            var totalCount = applicationAttributeDisplayables.Count();
            var fieldsToShow = applicationAttributeDisplayables.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var compositionData = new EntityRepository.SearchEntityResult();
            compositionData.PaginationData = new PaginatedSearchRequestDto(totalCount, pageNumber, pageSize, null, new List<int>() { pageSize });
            compositionData.ResultList = new List<Dictionary<string, object>>();
            foreach (var field in fieldsToShow) {
                var dict = new Dictionary<string, object>();
                dict["#label"] = string.IsNullOrEmpty(field.Label) ? field.Attribute : field.Label;
                dict["fieldKey"] = field.Attribute;
                //enabled by default
                if (field.IsReadOnly) {
                    dict["permission"] = "readonly";
                } else {
                    dict["permission"] = "fullcontrol";
                }
                if (field is ApplicationAssociationDefinition) {
                    var ass = (ApplicationAssociationDefinition)field;
                    if (ass.EnableExpression == "false") {
                        dict["permission"] = "readonly";
                    }
                }

                compositionData.ResultList.Add(dict);
            }
            return CompositionFetchResult.SingleCompositionInstance("#fieldPermissions_", compositionData);
        }

        [CanBeNull]
        public CompositionFetchResult LoadAvailableActionsAsComposition(ApplicationSchemaDefinition schema, int pageNumber) {
            var pageSize = 8;
            CommandBarDefinition commandBar;
            schema.CommandSchema.ApplicationCommands.TryGetValue("#actions", out commandBar);
            if (commandBar == null) {
                return null;
            }

            var commands = commandBar.Commands;
            var actionsToShow = commands.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            var totalCount = commands.Count();

            var compositionData = new EntityRepository.SearchEntityResult();
            compositionData.PaginationData = new PaginatedSearchRequestDto(totalCount, pageNumber, pageSize, null, new List<int>() { pageSize });
            compositionData.ResultList = new List<Dictionary<string, object>>();

            AddActionsToComposition(actionsToShow, compositionData);
            return CompositionFetchResult.SingleCompositionInstance("#actionPermissions_", compositionData);
        }

        private static void AddActionsToComposition(IEnumerable<ICommandDisplayable> actionsToShow, EntityRepository.SearchEntityResult compositionData) {
            foreach (var action in actionsToShow) {
                if (action is ContainerCommand) {
                    var con = (ContainerCommand)action;
                    AddActionsToComposition(con.Displayables, compositionData);
                } else {
                    var dict = new Dictionary<string, object>();
                    dict["#actionlabel"] = action.Label;
                    dict["actionid"] = action.Id;
                    //enabled by default
                    dict["_#selected"] = true;
                    compositionData.ResultList.Add(dict);
                }
            }
        }

        public MergedUserProfile BuildMergedProfile(List<UserProfile> profiles) {

            var permissionsDict = new Dictionary<string, ApplicationPermission>();

            foreach (var profile in profiles) {
                //let´s use the less restictive rule
                foreach (var appPermission in profile.ApplicationPermissions) {
                    if (!permissionsDict.ContainsKey(appPermission.ApplicationName)) {
                        permissionsDict.Add(appPermission.ApplicationName, appPermission);
                    } else {
                        permissionsDict[appPermission.ApplicationName].Merge(appPermission);
                    }
                }
            }

            var roles = new HashedSet<Role>();

            foreach (var profile in profiles) {
                roles.AddAll(profile.Roles);
            }

            return new MergedUserProfile() {
                Permissions = new HashedSet<ApplicationPermission>(permissionsDict.Values),
                Roles = roles
            };
        }
    }
}
