using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using cts.commons.Util;
using NHibernate;
using log4net;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.Persistence.Relational.DataConstraint;
using softWrench.sW4.Data.Persistence.SWDB;


namespace softWrench.sW4.Security.Services {
    internal class UserPreferenceManager {

        private static readonly ILog Log = LogManager.GetLogger(typeof(UserProfileManager));

        private static SWDBHibernateDAO DAO {
            get {
                return SWDBHibernateDAO.GetInstance();
            }
        }

        public static UserPreferences FindUserPreferences(User dbUser) {
            var preferences = DAO.FindSingleByQuery<UserPreferences>(UserPreferences.PreferenesByUserId, dbUser.Id);
            return preferences;
        }

        public static UserPreferences FindByUserId(int id) {
            var preferences = DAO.FindSingleByQuery<UserPreferences>(UserPreferences.PreferenesByUserId, id);
            return preferences;
        }

        public static UserPreferences SaveUserPreferences(UserPreferences preferences) {
            preferences = DAO.Save(preferences);
            return preferences;
        }
    }
}
