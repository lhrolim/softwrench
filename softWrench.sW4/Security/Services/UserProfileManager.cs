using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.Util;
using JetBrains.Annotations;
using NHibernate;
using log4net;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sw4.user.classes.entities;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.DataConstraint;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;


namespace softWrench.sW4.Security.Services {
    public class UserProfileManager : ISingletonComponent {

        private static readonly ILog Log = LogManager.GetLogger(typeof(UserProfileManager));

        private static readonly IDictionary<int?, UserProfile> ProfileCache = new Dictionary<int?, UserProfile>();

        private static bool _cacheStarted = false;

        private static readonly DataConstraintValidator ConstraintValidator = new DataConstraintValidator();

        private readonly SWDBHibernateDAO _dao;

        public UserProfileManager(SWDBHibernateDAO dao) {
            _dao = dao;
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
                using (var transaction = session.BeginTransaction()) {
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
            var sb = new StringBuilder();
            if (sb.Length > 0) {
                throw new InvalidOperationException(string.Format("Error saving constraints. Stack trace {0}", sb.ToString()));
            }
            profile = _dao.Save(profile);
            if (isUpdate && ProfileCache.ContainsKey(profile.Id)) {
                ProfileCache.Remove(profile.Id);
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
        public void DeleteProfile(UserProfile profile) {
            using (ISession session = SWDBHibernateDAO.GetInstance().GetSession()) {
                using (ITransaction transaction = session.BeginTransaction()) {
                    _dao.Delete(profile);
                    if (ProfileCache.ContainsKey(profile.Id)) {
                        ProfileCache.Remove(profile.Id);
                    }
                }
            }
        }

        public CompositionFetchResult LoadAvailableFieldsAsCompositionData(ApplicationSchemaDefinition schema, string tab, int pageNumber) {
            var fields = schema.NonHiddenFieldsOfTab(tab).Where(f=> !"true".Equals(f.RequiredExpression));
            var pageSize = 10;
            var applicationAttributeDisplayables = fields as IApplicationAttributeDisplayable[] ?? fields.ToArray();
            var totalCount = applicationAttributeDisplayables.Count();
            var fieldsToShow = applicationAttributeDisplayables.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var compositionData = new EntityRepository.SearchEntityResult();
            compositionData.PaginationData = new PaginatedSearchRequestDto(totalCount, pageNumber, pageSize, null, new List<int>() { pageSize });
            compositionData.ResultList = new List<Dictionary<string, object>>();
            foreach (var field in fieldsToShow) {
                var dict = new Dictionary<string, object>();
                dict["#label"] = field.Label ?? field.Attribute;
                dict["fieldKey"] = field.Attribute;
                //enabled by default
                dict["permission"] = "fullcontrol";
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
    }
}
