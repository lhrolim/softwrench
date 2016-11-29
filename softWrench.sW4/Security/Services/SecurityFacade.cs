﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.Remoting.Messaging;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Security;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using cts.commons.Util;
using JetBrains.Annotations;
using log4net;
using softwrench.sw4.user.classes.config;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.services;
using softwrench.sw4.user.classes.services.setup;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Entities.SyncManagers;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Preferences;
using softWrench.sW4.Util;
using LogicalThreadContext = Quartz.Util.LogicalThreadContext;
using softWrench.sW4.Util.TransactionStatistics;

namespace softWrench.sW4.Security.Services {
    public class SecurityFacade : ISingletonComponent, ISWEventListener<UserSavedEvent> {
        private const string LoginQuery = "from User where userName =? and IsActive=1";

        private static SecurityFacade _instance = null;

        private static IEventDispatcher _eventDispatcher;

        private static GridFilterManager _gridFilterManager;

        private static UserProfileManager _userProfileManager;

        private static UserSyncManager _userSyncManager;

        private static UserManager _userManager;

        private static UserStatisticsService _statisticsService;

        private static TransactionStatisticsService _transStatisticsService;

        private static readonly ILog Log = LogManager.GetLogger(typeof(SecurityFacade));

        private static readonly IDictionary<string, InMemoryUser> Users = new ConcurrentDictionary<string, InMemoryUser>();

        [Import]
        public UserPasswordService UserPasswordService { get; set; }

        public static SecurityFacade GetInstance() {
            return SimpleInjectorGenericFactory.Instance.GetObject<SecurityFacade>(typeof(SecurityFacade));
        }

        public SecurityFacade(IEventDispatcher dispatcher, GridFilterManager gridFilterManager, UserStatisticsService statisticsService, UserProfileManager userProfileManager, UserSyncManager userSyncManager, UserManager userManager, TransactionStatisticsService transStatisticsService, IConfigurationFacade configurationFacade) {
            _eventDispatcher = dispatcher;
            _gridFilterManager = gridFilterManager;
            _statisticsService = statisticsService;
            _userProfileManager = userProfileManager;
            _userSyncManager = userSyncManager;
            _userManager = userManager;
            _transStatisticsService = transStatisticsService;
        }

        [CanBeNull]
        public async Task<InMemoryUser> DoLogin(User dbUser, string userTimezoneOffset) {

            if (!dbUser.UserName.Equals("swadmin") && dbUser.IsActive.HasValue && dbUser.IsActive == false) {
                //swadmin cannot be inactive--> returning a non active user, so that we can differentiate it from a not found user on screen
                return InMemoryUser.NewAnonymousInstance(false, dbUser.Locked.HasValue && dbUser.Locked.Value);
            }
            //ensuring to get a clear instance upon login
            ClearUserFromCache(dbUser.UserName);

            if (dbUser.Systemuser || dbUser.MaximoPersonId == null) {
                //no need to sync to maximo, since there´s no such maximoPersonId
                return UserFound(dbUser, userTimezoneOffset);
            }
            var maximoUser = await _userSyncManager.GetUserFromMaximoBySwUser(dbUser);
            if (maximoUser == null) {
                Log.WarnFormat("maximo user {0} not found", dbUser.MaximoPersonId);
                return null;
            }
            maximoUser.MergeFromDBUser(dbUser);
            return UserFound(maximoUser, userTimezoneOffset);
        }

        [CanBeNull]
        public async Task<InMemoryUser> LoginCheckingPassword(User dbUser, string typedPassword, string userTimezoneOffset) {
            if (dbUser == null || !await UserPasswordService.MatchPassword(dbUser, typedPassword)) {

                return null;
            }
            return await DoLogin(dbUser, userTimezoneOffset);
        }


        public static void Logout(string username) {
            if (Users.ContainsKey(username)) {
                _transStatisticsService.CloseSessionAudit(Users[username].SessionAuditId);
                Users.Remove(username);
            }
        }


    

        public static InMemoryUser UpdateUserCacheStatic(User dbUser, string userTimezoneOffset) {
            int? userTimezoneOffsetInt = null;
            int tmp;
            if (int.TryParse(userTimezoneOffset, out tmp)) {
                userTimezoneOffsetInt = tmp;
            }

            var profiles = AsyncHelper.RunSync(() => _userProfileManager.FindUserProfiles(dbUser));
            var gridPreferences = new GridPreferences() {
                GridFilters = _gridFilterManager.LoadAllOfUser(dbUser.Id)
            };
            var userPreferences = UserPreferenceManager.FindUserPreferences(dbUser);
            var mergedProfile = _userProfileManager.BuildMergedProfile(profiles);

            var inMemoryUser = new InMemoryUser(dbUser, profiles, gridPreferences, userPreferences, userTimezoneOffsetInt, mergedProfile);
            if (Users.ContainsKey(inMemoryUser.Login)) {
                //some generic properties are evaluated upon login, so we might need to keep the old properties in case we´re not doing a login workflow
                //TODO: refactor
                inMemoryUser.Genericproperties = Users[inMemoryUser.Login].Genericproperties;
                Users.Remove(inMemoryUser.Login);
            }
            try {
                Users.Add(inMemoryUser.Login, inMemoryUser);
            } catch {
                Log.Warn("Duplicate user should not happen here " + inMemoryUser.Login);
            }
            return inMemoryUser;
        }

        public InMemoryUser UpdateUserCache(User dbUser, string userTimezoneOffset) {
            return UpdateUserCacheStatic(dbUser, userTimezoneOffset);
        }



        private static InMemoryUser UserFound(User dbUser, string userTimezoneOffset) {
            var inMemoryUser = UpdateUserCacheStatic(dbUser, userTimezoneOffset);
            _eventDispatcher.Dispatch(new UserLoginEvent(inMemoryUser));
            if (Log.IsDebugEnabled) {
                Log.Debug(String.Format("user:{0} logged in with roles {1}; profiles {2}; locations {3}",
                    inMemoryUser.Login,
                    string.Join(",", inMemoryUser.Roles),
                    string.Join(",", inMemoryUser.Profiles),
                    string.Join(",", inMemoryUser.PersonGroups)
                    ));
            }

            if (inMemoryUser.UserId.HasValue) {
                _transStatisticsService.AddSessionAudit(inMemoryUser);
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
        /// <returns>current autheticated user</returns>
        public static InMemoryUser CurrentUser(Boolean fetchFromDB = true) {
            if (ApplicationConfiguration.IsUnitTest) {
                return InMemoryUser.TestInstance("test");
            }

            if ("true".Equals(CallContext.LogicalGetData("executinglogin"))) {
                //in the middle of the logging in process
                return null;
            }

            var currLogin = LogicalThreadContext.GetData<string>("user") ?? CurrentPrincipalLogin;
            if (string.IsNullOrEmpty(currLogin)) {

                return InMemoryUser.NewAnonymousInstance();
            }

            if (!fetchFromDB || Users.ContainsKey(currLogin)) {
                var inMemoryUser = Users[currLogin];
                if (inMemoryUser == null) {
                    throw UnauthorizedException.NotAuthenticated(currLogin);
                }
                Log.DebugFormat("retrieving user {0} from cache", inMemoryUser.Login);
                return inMemoryUser;
            }
            //cookie authenticated already 
            //TODO: remove this in prod?
            var swUser = SWDBHibernateDAO.GetInstance().FindSingleByQuery<User>(User.UserByUserName, currLogin);
            if (swUser == null) {
                throw UnauthorizedException.UserNotFound(currLogin);
            }
            var fullUser = new User();
            CallContext.LogicalSetData("executinglogin", "true");
            if (!swUser.Systemuser && swUser.MaximoPersonId != null) {
                //TODO: Async this method
                fullUser = AsyncHelper.RunSync(() => _userSyncManager.GetUserFromMaximoBySwUser(swUser));
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
            CallContext.FreeNamedDataSlot("executinglogin");
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

        public User SaveUser(User user, ISet<UserProfile> profiles, ISet<UserCustomRole> customRoles, ISet<UserCustomConstraint> customConstraints) {
            user.Profiles = profiles;
            user.CustomRoles = customRoles;
            user.CustomConstraints = customConstraints;
            user.UserName = user.UserName.ToLower();
            return SWDBHibernateDAO.GetInstance().Save(user);
        }

        public async Task<User> SaveUser(User user) {
            var loginUser = CurrentUser();

            user.UserName = user.UserName.ToLower();
            var saveUser = await _userManager.SaveUser(user);
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

        public async Task<User> FetchUser(int id) {
            var swUser = UserManager.GetUserById(id);
            var maximoUser = await _userSyncManager.GetUserFromMaximoBySwUser(swUser);
            if (maximoUser == null) {
                return swUser;
            }
            return maximoUser;
        }


        public void HandleEvent(UserSavedEvent userEvent) {
            Users.Remove(userEvent.Login);
        }
    }
}
