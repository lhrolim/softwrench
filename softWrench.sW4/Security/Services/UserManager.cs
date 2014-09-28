using Iesi.Collections.Generic;
using softWrench.sW4.Data.Entities.SyncManagers;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softWrench.sW4.Security.Services {
    public class UserManager : ISingletonComponent {
        private const string HlagPrefix = "@HLAG.COM";
        private static SWDBHibernateDAO _dao = new SWDBHibernateDAO();

        public static User SaveUser(User user) {
            if (user.Id != null) {
                var dbuser = _dao.FindByPK<User>(typeof(User), (int)user.Id);
                user.MergeFromDBUser(dbuser);

            }
            return _dao.Save(user);
        }

        public static User CreateMissingDBUser(string userName, bool save = true) {
            var personid = userName.ToUpper();
            if (IsHapagProd) {
                if (!personid.EndsWith(HlagPrefix)) {
                    personid += HlagPrefix;
                }
            }
            var user = UserSyncManager.GetUserFromMaximoByUserName(personid);
            if (user == null) {
                //if the user does not exist on maximo, then we should not create it on softwrench either
                return null;
            }
            if (string.IsNullOrEmpty(user.UserName)) {
                user.UserName = personid;
            }
            if (IsHapagProd) {
                if (personid.EndsWith(HlagPrefix)) {
                    user.UserName = personid.Substring(0, personid.Length - HlagPrefix.Length);
                }
            }

            user.CustomRoles = new HashedSet<UserCustomRole>();
            return save ? _dao.Save(user) : user;
        }

        public static User SyncLdapUser(User existingUser, bool isLdapSetup) {
            if (existingUser.MaximoPersonId == null) {
                return existingUser;
            }

            var user = UserSyncManager.GetUserFromMaximoByUserName(existingUser.MaximoPersonId);
            if (user == null) {
                return existingUser;
            }
            existingUser.MergeMaximoWithNewUser(user);
            if (isLdapSetup) {
                //let's play safe and clean passwords of users that might have been wrongly stored on swdb database, if we are on ldap auth
                existingUser.Password = null;
            }
            return _dao.Save(existingUser);
        }

        private static bool IsHapagProd {
            get { return ApplicationConfiguration.ClientName == "hapag" && ApplicationConfiguration.IsProd(); }
        }

    }
}
