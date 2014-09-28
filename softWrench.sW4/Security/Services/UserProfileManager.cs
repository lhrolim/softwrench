using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NHibernate;
using log4net;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.DataConstraint;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Util;


namespace softWrench.sW4.Security.Services {
    internal class UserProfileManager {

        private static readonly log4net.ILog Log = LogManager.GetLogger(typeof(UserProfileManager));

        private static readonly IDictionary<int?, UserProfile> ProfileCache = new Dictionary<int?, UserProfile>();
        private static readonly DataConstraintValidator ConstraintValidator = new DataConstraintValidator();
        private static readonly SWDBHibernateDAO DAO = new SWDBHibernateDAO();

        public static UserProfile FindByName(String name) {
            if (ProfileCache.Count == 0) {
                FetchAllProfiles(true);
            }

            foreach (var userProfile in ProfileCache.Values) {
                if (userProfile.Name.Equals(name.ToLower(), StringComparison.CurrentCultureIgnoreCase)) {
                    return userProfile;
                }
            }
            return null;
        }

        public static ICollection<UserProfile> FetchAllProfiles(Boolean eager) {
            var before = Stopwatch.StartNew();
            if (ProfileCache.Count != 0) {
                return ProfileCache.Values;
            }

            using (var session = SWDBHibernateDAO.CurrentSession()) {
                using (var transaction = session.BeginTransaction()) {
                    var query = DAO.BuildQuery("from UserProfile", (object[])null, session);
                    var dbProfiles = query.List();
                    var profiles = new Dictionary<string, UserProfile>();
                    foreach (var profile in dbProfiles) {
                        var userProfile = (UserProfile)profile;
                        if (eager) {
                            NHibernateUtil.Initialize(userProfile.Roles);
                            NHibernateUtil.Initialize(userProfile.DataConstraints);
                        }
                        profiles.Add(userProfile.Name, userProfile);
                        ProfileCache.Add(userProfile.Id, userProfile);
                    }
                    Log.Info(LoggingUtil.BaseDurationMessage("Profiles Loaded in {0}", before));
                    return profiles.Values;
                }
            }

        }

        public static UserProfile SaveUserProfile(UserProfile profile) {
            var isUpdate = profile.Id != null;
            var sb = new StringBuilder();
            foreach (var dataConstraint in profile.DataConstraints) {
                var message = ConstraintValidator.ValidateContraint(dataConstraint);
                if (message != null) {
                    sb.Append(message).Append("\n");
                }
            }
            if (sb.Length > 0) {
                throw new InvalidOperationException(String.Format("Error saving constraints. Stack trace {0}", sb.ToString()));
            }
            profile = DAO.Save(profile);
            if (isUpdate && ProfileCache.ContainsKey(profile.Id)) {
                ProfileCache.Remove(profile.Id);
            }
            ProfileCache.Add(profile.Id, profile);
            return profile;
        }

        public static List<UserProfile> FindUserProfiles(User dbUser) {
            if (dbUser.Profiles == null) {
                return new List<UserProfile>();
            }
            return dbUser.Profiles.Select(profile => ProfileCache[profile.Id]).ToList();
        }

        //TODO: remove customUserRoles and customUSerCOnstraints which were exclusions from this profile ==> They don´t make sense anymore (tough,they are useless anyway)
        public static void DeleteProfile(UserProfile profile) {
            using (ISession session = SWDBHibernateDAO.CurrentSession()) {
                using (ITransaction transaction = session.BeginTransaction()) {
                    DAO.Delete(profile);
                    if (ProfileCache.ContainsKey(profile.Id)) {
                        ProfileCache.Remove(profile.Id);
                    }
                }
            }
        }


    }
}
