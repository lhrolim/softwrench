﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Principal;
using log4net;
using softWrench.sW4.AUTH;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Entities;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.SimpleInjector.Events;
using softWrench.sW4.Util;
using LogicalThreadContext = Quartz.Util.LogicalThreadContext;


namespace softWrench.sW4.Security.Services {
    public class SecurityFacade {
        private const string LoginQuery = "from User where userName =? and IsActive=true";

        private static SecurityFacade _instance = null;

        private static IEventDispatcher _eventDispatcher;

        private static readonly ILog Log = LogManager.GetLogger(typeof(SecurityFacade));


        public static SecurityFacade GetInstance() {
            if (_instance == null) {
                _instance = new SecurityFacade();
            }
            return _instance;
        }

        private SecurityFacade() {
            _eventDispatcher =
                SimpleInjectorGenericFactory.Instance.GetObject<IEventDispatcher>(typeof(IEventDispatcher));
        }


        public static readonly IDictionary<string, InMemoryUser> _users = new ConcurrentDictionary<string, InMemoryUser>();

        public InMemoryUser LdapLogin(User dbUser, string userTimezoneOffset) {
            return UserFound(dbUser, userTimezoneOffset);
        }

        public InMemoryUser Login(User dbUser, string typedPassword, string userTimezoneOffset) {
            if (dbUser == null || !MatchPassword(dbUser, typedPassword)) {
                return null;
            }
            return UserFound(dbUser, userTimezoneOffset);
        }

        public InMemoryUser Login(string userName, string password, string userTimezoneOffset) {
            var shaPassword = AuthUtils.GetSha1HashData(password);
            var md5Password = AuthUtils.GetHashData(password, SHA256.Create());
            var dbUser = new SWDBHibernateDAO().FindSingleByQuery<User>(LoginQuery, userName);
            if (dbUser == null || !MatchPassword(dbUser, password)) {
                return null;
            }

            return UserFound(dbUser, userTimezoneOffset);
        }



        private bool MatchPassword(User dbUser, string password) {
            var encryptedPassword = "";
            string criptoProperties = dbUser.CriptoProperties;
            if (String.IsNullOrEmpty(criptoProperties)) {
                encryptedPassword = AuthUtils.GetSha1HashData(password);
            }
            //TODO: read criptoProperties and apply custom logic
            return dbUser.Password.Equals(encryptedPassword);
        }

        private static InMemoryUser UserFound(User dbUser, string userTimezoneOffset) {

            int? userTimezoneOffsetInt = null;
            int tmp;
            if (Int32.TryParse(userTimezoneOffset, out tmp)) {
                userTimezoneOffsetInt = tmp;
            }

            var profiles = UserProfileManager.FindUserProfiles(dbUser);
            var inMemoryUser = new InMemoryUser(dbUser, profiles, userTimezoneOffsetInt);
            if (_users.ContainsKey(inMemoryUser.Login)) {
                _users.Remove(inMemoryUser.Login);
            }
            try {
                _users.Add(inMemoryUser.Login, inMemoryUser);
            } catch {
                Log.Warn("Duplicate user should not happen here " + inMemoryUser.Login);
            }
            LogicalThreadContext.SetData("user", dbUser.UserName);
            _eventDispatcher.Dispatch(new UserLoginEvent(inMemoryUser));
            if (Log.IsDebugEnabled) {
                Log.Debug(String.Format("user:{0} logged in with roles {1}; profiles {2}; locations {3}",
                    inMemoryUser.Login,
                    string.Join(",", inMemoryUser.Roles),
                    string.Join(",", inMemoryUser.Profiles),
                    string.Join(",", inMemoryUser.PersonGroups)
                    ));
            }
            return inMemoryUser;
        }


        public void SaveUpdateRole(Role role) {
            RoleManager.SaveUpdateRole(role);
        }

        public void DeleteRole(Role role) {
            RoleManager.DeleteRole(role);
        }

        public static void InitSecurity() {
            RoleManager.LoadActiveRoles();
            UserProfileManager.FetchAllProfiles(true);
        }



        public static IPrincipal CurrentPrincipal {
            get { return System.Threading.Thread.CurrentPrincipal; }
        }

        public static string CurrentPrincipalLogin {
            get { return CurrentPrincipal.Identity.Name; }
        }


        public static InMemoryUser CurrentUser(Boolean fetchFromDB = true, bool throwAnonymousException = true) {
            if (ApplicationConfiguration.IsUnitTest) {
                return InMemoryUser.TestInstance("test");
            }

            var currLogin = LogicalThreadContext.GetData<string>("user") ?? CurrentPrincipalLogin;
            if (currLogin == "") {
                if (!throwAnonymousException) {
                    return new InMemoryUser { Login = "Anonymous" };
                }
                throw new UnauthorizedAccessException();
            }

            if (!fetchFromDB || currLogin == null || _users.ContainsKey(currLogin)) {
                return _users[currLogin];
            }
            //cookie authenticated already 
            //TODO: remove this in prod?
            var dbUser = new SWDBHibernateDAO().FindSingleByQuery<User>(User.UserByUserName, currLogin);
            if (dbUser == null) {
                throw new InvalidOperationException("user should exist at DB");
            }

            var formsIdentity = CurrentPrincipal.Identity as System.Web.Security.FormsIdentity;
            var timezone = String.Empty;
            if (formsIdentity != null && formsIdentity.Ticket != null && !String.IsNullOrWhiteSpace(formsIdentity.Ticket.UserData)) {
                var userData = PropertyUtil.ConvertToDictionary(formsIdentity.Ticket.UserData);
                timezone = userData["userTimezoneOffset"];
            }
            UserFound(dbUser, timezone);
            return _users[currLogin];
        }

        //TODO: this could lead to concurrency problems
        public static void ClearUserFromCache(String login = null, InMemoryUser userToPut = null) {
            //this means, an action that affects all the users, like updating a profile
            if (login == null) {
                _users.Clear();
            } else if (_users.ContainsKey(login)) {
                _users.Remove(login);
            }
            if (userToPut != null) {
                _users[login] = userToPut;
            }
        }


        public void SaveUserProfile(UserProfile profile) {
            UserProfileManager.SaveUserProfile(profile);
            ClearUserFromCache();
        }


        public ICollection<UserProfile> FetchAllProfiles(Boolean eager) {
            return UserProfileManager.FetchAllProfiles(eager);
        }

        public void DeleteProfile(UserProfile profile) {
            UserProfileManager.DeleteProfile(profile);
        }
        public User SaveUser(User user, Iesi.Collections.Generic.ISet<UserProfile> profiles, Iesi.Collections.Generic.ISet<UserCustomRole> customRoles,
            Iesi.Collections.Generic.ISet<UserCustomConstraint> customConstraints) {
            user.Profiles = profiles;
            user.CustomRoles = customRoles;
            user.CustomConstraints = customConstraints;
            user.UserName = user.UserName.ToLower();
            return new SWDBHibernateDAO().Save(user);
        }

        public User SaveUser(User user) {
            user.UserName = user.UserName.ToLower();
            var saveUser = UserManager.SaveUser(user);
            ClearUserFromCache(user.UserName);
            return saveUser;
        }

        public void DeleteUser(User user) {
            new SWDBHibernateDAO().Delete(user);
        }

        public User FetchUser(int id) {
            return new SWDBHibernateDAO().FindByPK<User>(typeof(User), id, "Profiles", "CustomRoles", "CustomConstraints");
        }
    }
}
