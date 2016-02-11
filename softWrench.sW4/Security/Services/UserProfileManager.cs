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
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.Persistence.Relational.DataConstraint;
using softWrench.sW4.Data.Persistence.SWDB;


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


    }
}
