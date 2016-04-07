using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Web.Security;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using cts.commons.Util;
using log4net;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.services;
using softwrench.sw4.user.classes.services.setup;
using softWrench.sW4.Data.Entities.SyncManagers;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Preferences;
using softWrench.sW4.Util;
using LogicalThreadContext = Quartz.Util.LogicalThreadContext;


namespace softWrench.sW4.Security.Services {
    public class SecurityFacade : ISingletonComponent, ISWEventListener<UserSavedEvent> {
        private const string LoginQuery = "from User where userName =? and IsActive=1";

        private static SecurityFacade _instance = null;

        private static IEventDispatcher _eventDispatcher;

        private static GridFilterManager _gridFilterManager;

        private static UserProfileManager _userProfileManager;

        private static UserStatisticsService _statisticsService;

        private static readonly ILog Log = LogManager.GetLogger(typeof(SecurityFacade));

        private static readonly IDictionary<string, InMemoryUser> Users = new ConcurrentDictionary<string, InMemoryUser>();


        public static SecurityFacade GetInstance() {
            if (_instance == null) {
                _instance = SimpleInjectorGenericFactory.Instance.GetObject<SecurityFacade>(typeof(SecurityFacade));
            }
            return _instance;
        }

        public SecurityFacade(IEventDispatcher dispatcher, GridFilterManager gridFilterManager, UserStatisticsService statisticsService, UserProfileManager userProfileManager) {
            _eventDispatcher = dispatcher;
            _gridFilterManager = gridFilterManager;
            _statisticsService = statisticsService;
            _userProfileManager = userProfileManager;
        }

        public InMemoryUser DoLogin(User dbUser, string userTimezoneOffset) {
            if (dbUser.Systemuser || dbUser.MaximoPersonId == null) {
                //no need to sync to maximo, since there´s no such maximoPersonId
                return UserFound(dbUser, userTimezoneOffset);
            }
            var maximoUser = UserSyncManager.GetUserFromMaximoByUserName(dbUser.MaximoPersonId, dbUser.Id);
            if (maximoUser == null) {
                Log.WarnFormat("maximo user {0} not found",dbUser.MaximoPersonId);
                return UserFound(dbUser, userTimezoneOffset);
            }
            maximoUser.MergeFromDBUser(dbUser);


            return UserFound(maximoUser, userTimezoneOffset);
        }

        public InMemoryUser Login(User dbUser, string typedPassword, string userTimezoneOffset) {
            if (dbUser == null || !MatchPassword(dbUser, typedPassword)) {
                return null;
            }
            var maximoUser = UserSyncManager.GetUserFromMaximoByUserName(dbUser.UserName, dbUser.Id);
            if (maximoUser == null) {
                if (dbUser.UserName.ToLower() == "swadmin") {
                    return UserFound(dbUser, userTimezoneOffset);
                }

                Log.WarnFormat("user {0} not found on maximo with maximopersonid {1}, login unauthorized", dbUser.UserName, dbUser.MaximoPersonId);
                return null;
            }
            maximoUser.MergeFromDBUser(dbUser);
            return UserFound(maximoUser, userTimezoneOffset);
        }

        public InMemoryUser Login(string userName, string password, string userTimezoneOffset) {
            var dbUser = SWDBHibernateDAO.GetInstance().FindSingleByQuery<User>(LoginQuery, userName);
            if (dbUser == null || !MatchPassword(dbUser, password)) {
                return null;
            }

            dbUser = UserSyncManager.GetUserFromMaximoByUserName(userName, dbUser.Id);

            return UserFound(dbUser, userTimezoneOffset);
        }



        private bool MatchPassword(User dbUser, string password) {
            var encryptedPassword = "";
            var criptoProperties = dbUser.CriptoProperties;
            if (String.IsNullOrEmpty(criptoProperties)) {
                encryptedPassword = AuthUtils.GetSha1HashData(password);
            }
            //TODO: read criptoProperties and apply custom logic
            return dbUser.Password.Equals(encryptedPassword);
        }

        public static InMemoryUser UpdateUserCache(User dbUser, string userTimezoneOffset) {
            int? userTimezoneOffsetInt = null;
            int tmp;
            if (int.TryParse(userTimezoneOffset, out tmp)) {
                userTimezoneOffsetInt = tmp;
            }

            var profiles = _userProfileManager.FindUserProfiles(dbUser);
            var gridPreferences = new GridPreferences() {
                GridFilters = _gridFilterManager.LoadAllOfUser(dbUser.Id)
            };
            var userPreferences = UserPreferenceManager.FindUserPreferences(dbUser);
            var mergedProfile = _userProfileManager.BuildMergedProfile(profiles);

            var inMemoryUser = new InMemoryUser(dbUser, profiles, gridPreferences, userPreferences, userTimezoneOffsetInt, mergedProfile);
            if (Users.ContainsKey(inMemoryUser.Login)) {
                Users.Remove(inMemoryUser.Login);
            }
            try {
                Users.Add(inMemoryUser.Login, inMemoryUser);
            } catch {
                Log.Warn("Duplicate user should not happen here " + inMemoryUser.Login);
            }
            return inMemoryUser;
        }

        private static InMemoryUser UserFound(User dbUser, string userTimezoneOffset) {
            var inMemoryUser = UpdateUserCache(dbUser, userTimezoneOffset);
            _eventDispatcher.Dispatch(new UserLoginEvent(inMemoryUser));
            if (Log.IsDebugEnabled) {
                Log.Debug(String.Format("user:{0} logged in with roles {1}; profiles {2}; locations {3}",
                    inMemoryUser.Login,
                    string.Join(",", inMemoryUser.Roles),
                    string.Join(",", inMemoryUser.Profiles),
                    string.Join(",", inMemoryUser.PersonGroups)
                    ));
            }
            _statisticsService.UpdateStatistcsAsync(dbUser);
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
            _userProfileManager.FetchAllProfiles(true);
        }



        public static IPrincipal CurrentPrincipal {
            get {
                return Thread.CurrentPrincipal;
            }
        }

        public static string CurrentPrincipalLogin {
            get {
                return CurrentPrincipal.Identity.Name;
            }
        }
        /// <summary>
        /// <para>
        /// Finds the currently authenticated user.
        /// </para>
        /// <para>
        /// If requiresAuth is <code>true</code> and there is no authenticated user it will throw a <see cref="UnauthorizedException"/>.
        /// </para>
        /// <para>
        /// If requiresAuth is <code>false</code> and there is no authenticated user it returns an 'anonymous' user instance (<see cref="InMemoryUser.NewAnonymousInstance"/>)
        /// </para>
        /// </summary>
        /// <param name="fetchFromDB"></param>
        /// <param name="requiresAuth"></param>
        /// <returns>current autheticated user</returns>
        public static InMemoryUser CurrentUser(Boolean fetchFromDB = true, Boolean requiresAuth = false) {
            if (ApplicationConfiguration.IsUnitTest) {
                return InMemoryUser.TestInstance("test");
            }

            if ("true".Equals(LogicalThreadContext.GetData<string>("executinglogin"))) {
                //in the middle of the logging in process
                return null;
            }

            var currLogin = LogicalThreadContext.GetData<string>("user") ?? CurrentPrincipalLogin;
            if (string.IsNullOrEmpty(currLogin)) {
                if (requiresAuth) {
                    throw UnauthorizedException.NotAuthenticated(currLogin);
                }
                return InMemoryUser.NewAnonymousInstance();
            }

            if (!fetchFromDB || Users.ContainsKey(currLogin)) {
                var inMemoryUser = Users[currLogin];
                if (inMemoryUser == null) {
                    throw UnauthorizedException.NotAuthenticated(currLogin);
                }
                return inMemoryUser;
            }
            //cookie authenticated already 
            //TODO: remove this in prod?
            var swUser = SWDBHibernateDAO.GetInstance().FindSingleByQuery<User>(User.UserByUserName, currLogin);
            if (swUser == null) {
                throw UnauthorizedException.UserNotFound(currLogin);
            }
            var fullUser = new User();
            LogicalThreadContext.SetData("executinglogin", "true");
            if (!swUser.Systemuser && swUser.MaximoPersonId != null) {
                fullUser = UserSyncManager.GetUserFromMaximoByUserName(currLogin, swUser.Id, true);
                if (fullUser == null) {
                    Log.WarnFormat("user {0} not found on database", swUser.MaximoPersonId);
                    fullUser = new User();
                }
            }
            fullUser.MergeFromDBUser(swUser);
            var formsIdentity = CurrentPrincipal.Identity as FormsIdentity;
            var timezone = String.Empty;
            if (formsIdentity != null && formsIdentity.Ticket != null && !String.IsNullOrWhiteSpace(formsIdentity.Ticket.UserData)) {
                var userData = PropertyUtil.ConvertToDictionary(formsIdentity.Ticket.UserData);
                timezone = userData["userTimezoneOffset"] as string;
            }
            UserFound(fullUser, timezone);
            LogicalThreadContext.FreeNamedDataSlot("executinglogin");
            var currentUser = Users[currLogin];
            if (currentUser == null) {
                throw UnauthorizedException.NotAuthenticated(currLogin);
            }
            return currentUser;
        }

        //TODO: this could lead to concurrency problems
        public static void ClearUserFromCache(string login = null, InMemoryUser userToPut = null) {
            //this means, an action that affects all the users, like updating a profile
            if (login == null) {
                Users.Clear();
            } else if (Users.ContainsKey(login)) {
                Users.Remove(login);
            }
            if (userToPut != null) {
                Users[login] = userToPut;
            }
        }


        public void SaveUserProfile(UserProfile profile) {
            _userProfileManager.SaveUserProfile(profile);
            ClearUserFromCache();
        }


        public ICollection<UserProfile> FetchAllProfiles(Boolean eager) {
            return _userProfileManager.FetchAllProfiles(eager);
        }

        public User SaveUser(User user, Iesi.Collections.Generic.ISet<UserProfile> profiles, Iesi.Collections.Generic.ISet<UserCustomRole> customRoles, Iesi.Collections.Generic.ISet<UserCustomConstraint> customConstraints) {
            user.Profiles = profiles;
            user.CustomRoles = customRoles;
            user.CustomConstraints = customConstraints;
            user.UserName = user.UserName.ToLower();
            return SWDBHibernateDAO.GetInstance().Save(user);
        }

        public User SaveUser(User user) {
            var loginUser = CurrentUser();

            user.UserName = user.UserName.ToLower();
            var saveUser = UserManager.SaveUser(user);
            ClearUserFromCache(user.UserName);

            if (saveUser.UserName.Equals(loginUser.Login)) {
                var timezone = loginUser.TimezoneOffset.ToString();
                UserFound(user, timezone);
            }

            return saveUser;
        }

        public void DeleteUser(User user) {
            SWDBHibernateDAO.GetInstance().Delete(user);
        }

        public User FetchUser(int id) {
            User swUser = UserManager.GetUserById(id);
            return UserSyncManager.GetUserFromMaximoBySwUser(swUser);
        }

        public User FetchUser(string maximopersonid) {
            User swUser = UserManager.GetUserByUsername(maximopersonid);
            return UserSyncManager.GetUserFromMaximoBySwUser(swUser);
        }

        public void HandleEvent(UserSavedEvent userEvent) {
            Users.Remove(userEvent.Login);
        }
    }
}
