using System;
using System.Collections.Generic;
using System.Linq;
using Iesi.Collections.Generic;
using softWrench.sW4.Data.Entities.SyncManagers;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Entities;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using Quartz.Util;
using softWrench.sW4.AUTH;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Util;

namespace softWrench.sW4.Security.Services {
    public class UserManager : ISingletonComponent {
        private const string HlagPrefix = "@HLAG.COM";




        private static IEventDispatcher _dispatcher;

        public static IEventDispatcher Dispatcher {
            get {
                if (_dispatcher == null) {
                    _dispatcher =SimpleInjectorGenericFactory.Instance.GetObject<IEventDispatcher>(typeof(IEventDispatcher));
                }
                return _dispatcher;

            }
        }

        public static SWDBHibernateDAO DAO {
            get {
                return SWDBHibernateDAO.GetInstance();

            }
        }

        public static User SaveUser(User user, bool updateMaximo = false) {
            if (user.Id != null) {
                var dbuser = DAO.FindByPK<User>(typeof(User), (int)user.Id);
                user.MergeFromDBUser(dbuser);

            }
            if (updateMaximo) {
                user.Person.Save();
            }
            var savedUser = DAO.Save(user);
            // ReSharper disable once PossibleInvalidOperationException --> just saved
            Dispatcher.Dispatch(new UserSavedEvent(savedUser.Id.Value,savedUser.UserName));
            return savedUser;
        }

        public void DefineUserPassword(User user, string password) {
            user.Password = AuthUtils.GetSha1HashData(password);
            var savedUser =DAO.Save(user);
            //TODO: wipeout link
            // ReSharper disable once PossibleInvalidOperationException
            Dispatcher.Dispatch(new UserSavedEvent(savedUser.Id.Value, savedUser.UserName));
        }

        public User FindUserByLink(string tokenLink) {
            //TODO: implement and remove this mock
            return new User()
            {
                FirstName = "Luiz",
                LastName = "Rolim"
            };
        }


        public static User GetUserById(int id) {
            return SWDBHibernateDAO.GetInstance().FindByPK<User>(typeof(User), id, "Profiles", "CustomRoles", "CustomConstraints");
        }

        public static User GetUserByUsername(string username) {
            User user = SWDBHibernateDAO.GetInstance().FindSingleByQuery<User>(User.UserByUserName, username) ?? null;

            return user;
        }

        public static IEnumerable<User> GetUsersByUsername(List<string> usernames) {
            var param = BaseQueryUtil.GenerateInString(usernames);
            var querystring = string.Format("from User where lower(userName) in ({0})", param.ToLower());
            return SWDBHibernateDAO.GetInstance()
                .FindByQuery<User>(querystring);
        }

        public static User CreateMissingDBUser(string userName, bool save = true) {
            var personid = userName.ToUpper();
            if (IsHapagProd) {
                if (!personid.EndsWith(HlagPrefix)) {
                    personid += HlagPrefix;
                }
            }
            var user = UserSyncManager.GetUserFromMaximoByUserName(personid, null);
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
            return save ? DAO.Save(user) : user;
        }

        public static User SyncLdapUser(User existingUser, bool isLdapSetup) {
            if (existingUser.MaximoPersonId == null) {
                return existingUser;
            }

            var user = UserSyncManager.GetUserFromMaximoByUserName(existingUser.MaximoPersonId, existingUser.Id);
            if (user == null) {
                return existingUser;
            }
            existingUser.MergeMaximoWithNewUser(user);
            if (isLdapSetup) {
                //let's play safe and clean passwords of users that might have been wrongly stored on swdb database, if we are on ldap auth
                existingUser.Password = null;
            }
            return DAO.Save(existingUser);
        }

        private static bool IsHapagProd {
            get { return ApplicationConfiguration.ClientName == "hapag" && ApplicationConfiguration.IsProd(); }
        }

    }
}
