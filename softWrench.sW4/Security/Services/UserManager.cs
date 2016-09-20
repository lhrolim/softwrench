using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Iesi.Collections.Generic;
using softWrench.sW4.Data.Entities.SyncManagers;
using softWrench.sW4.Data.Persistence.SWDB;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using cts.commons.Util;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.services.setup;
using softWrench.sW4.AUTH;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Util;

namespace softWrench.sW4.Security.Services {
    public class UserManager : ISingletonComponent {

        private const string HlagPrefix = "@HLAG.COM";


        protected readonly UserLinkManager UserLinkManager;

        protected readonly MaximoHibernateDAO MaxDAO;

        protected readonly UserSetupEmailService UserSetupEmailService;

        protected readonly UserSyncManager UserSyncManager;

        private static LdapManager _ldapManager;

        private readonly IEventDispatcher _dispatcher;

        private readonly ISWDBHibernateDAO _dao;

        
        public UserManager(UserLinkManager userLinkManager, MaximoHibernateDAO maxDAO, UserSetupEmailService userSetupEmailService, LdapManager ldapManager, UserSyncManager userSyncManager, IEventDispatcher dispatcher, ISWDBHibernateDAO swdbDAO) {
            UserLinkManager = userLinkManager;
            MaxDAO = maxDAO;
            UserSetupEmailService = userSetupEmailService;
            _ldapManager = ldapManager;
            UserSyncManager = userSyncManager;
            _dispatcher = dispatcher;
            _dao = swdbDAO;
        }

      

    

        public virtual User SaveUser(User user, bool updateMaximo = false) {
            if (user.Id == null && user.MaximoPersonId == null) {
                user.MaximoPersonId = user.UserName;
            }
            if (updateMaximo) {
                user.Person.Save();
            }
            var savedUser = _dao.Save(user);
            // ReSharper disable once PossibleInvalidOperationException --> just saved
            _dispatcher.Dispatch(new UserSavedEvent(savedUser.Id.Value, savedUser.UserName));
            return savedUser;
        }

        public void ActivateAndDefinePassword(User user, string password) {
            user.Password = AuthUtils.GetSha1HashData(password);
            user.IsActive = true;
            user.ChangePassword = false;
            var savedUser = _dao.Save(user);
            //TODO: wipeout link
            UserLinkManager.DeleteLink(savedUser);
            // ReSharper disable once PossibleInvalidOperationException
            _dispatcher.Dispatch(new UserSavedEvent(savedUser.Id.Value, savedUser.UserName));
        }




        public static User GetUserById(int id) {
            return SWDBHibernateDAO.GetInstance().FindByPK<User>(typeof(User), id, "Profiles", "CustomRoles", "CustomConstraints");
        }

        public static User GetUserByUsername(string username) {
            User user = SWDBHibernateDAO.GetInstance().FindSingleByQuery<User>(User.UserByUserName, username) ?? null;

            return user;
        }

        public static User GetUserByPersonId(string maximoPersonId) {
            User user = SWDBHibernateDAO.GetInstance().FindSingleByQuery<User>(User.UserByMaximoPersonId, maximoPersonId) ?? null;

            return user;
        }

        public static IEnumerable<User> GetUserByPersonIds(IEnumerable<string> personIds) {
            return SWDBHibernateDAO.GetInstance().FindByQuery<User>(User.UserByMaximoPersonIds, personIds) ?? null;
        }

        public static IEnumerable<User> GetUsersByUsername(List<string> usernames) {
            var param = BaseQueryUtil.GenerateInString(usernames);
            var querystring = string.Format("from User where lower(userName) in ({0})", param.ToLower());
            return SWDBHibernateDAO.GetInstance()
                .FindByQuery<User>(querystring);
        }

        public virtual async Task<User> CreateMissingDBUser(string userName, bool save = true) {
            var personid = userName.ToUpper();
            if (IsHapagProd) {
                if (!personid.EndsWith(HlagPrefix)) {
                    personid += HlagPrefix;
                }
            }
            var user = await UserSyncManager.GetUserFromMaximoByPersonId(personid, true);
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
            user.IsActive = true;
            user.CustomRoles = new LinkedHashSet<UserCustomRole>();
            if (save) {
                try {
                    return await _dao.SaveAsync(user);
                } catch (Exception e) {
                    if (_ldapManager.IsLdapSetup()) {
                        //recovering from scenarios where there might have been an already created user on softwrench with a personid equal to an existing user in maximo
                        var legacyUser = _dao.FindSingleByQuery<User>(User.UserByUserNameOrPerson, user.UserName, user.MaximoPersonId);
                        if (legacyUser != null) {
                            legacyUser.UserName = user.UserName;
                            legacyUser.MaximoPersonId = user.MaximoPersonId;
                            return _dao.Save(legacyUser);
                        }
                    }
                    throw;
                }
            }

            return user;
        }

        public async Task<User> SyncLdapUser(User existingUser, bool isLdapSetup) {
            if (existingUser.MaximoPersonId == null || existingUser.Systemuser) {
                return existingUser;
            }

            var user = await UserSyncManager.GetUserFromMaximoBySwUser(existingUser);
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
            get {
                return ApplicationConfiguration.ClientName == "hapag" && ApplicationConfiguration.IsProd();
            }
        }

        public User FindUserByLink(string tokenLink, out bool hasExpired) {
            var user = UserLinkManager.RetrieveUserByLink(tokenLink, out hasExpired);
            if (user != null) {
                //TODO: Async refactoring
                var maximoUser = AsyncHelper.RunSync(()=> UserSyncManager.GetUserFromMaximoBySwUser(user));
                user.MergeMaximoWithNewUser(maximoUser);
            }
            return user;
        }

        public async Task<string> ForgotPassword(string userNameOrEmail) {
            var isEmail = new EmailAddressAttribute().IsValid(userNameOrEmail);
            User user;
            string emailToSend = null;
            if (!isEmail) {
                user = _dao.FindSingleByQuery<User>(User.UserByUserName, userNameOrEmail);
                if (user == null) {
                    return "User {0} not found".Fmt(userNameOrEmail);
                }
                var email = MaxDAO.FindSingleByNativeQuery<string>("select emailaddress from email where personid = ? and isPrimary = 1", user.MaximoPersonId);
                if (email == null) {
                    return "User {0} has no primary email registered. Please contact your administrator".Fmt(userNameOrEmail);
                }
                emailToSend = email;
            } else {
                emailToSend = userNameOrEmail;
                var personid = MaxDAO.FindSingleByNativeQuery<string>("select personid from email where emailaddress = ? and isPrimary = 1", userNameOrEmail);
                if (personid == null) {
                    return "The email {0} is not registered on the database".Fmt(userNameOrEmail);
                }
                user = _dao.FindSingleByQuery<User>(User.UserByMaximoPersonId, personid);
                if (user == null) {
                    return "The email {0} is not registered on the database".Fmt(userNameOrEmail);
                }
            }
            user = await UserSyncManager.GetUserFromMaximoBySwUserFallingBackToDefault(user);
            if (!user.IsPoPulated()) {
                return "User {0} not found".Fmt(userNameOrEmail);
            }
            UserSetupEmailService.ForgotPasswordEmail(user, emailToSend);
            return null;
        }

        public async void SendActivationEmail(int userId, string email) {
            var user = _dao.FindByPK<User>(typeof(User), userId);
            if (user == null) {
                throw new InvalidOperationException("user {0} not found".Fmt(userId));
            }
            user = await UserSyncManager.GetUserFromMaximoBySwUserFallingBackToDefault(user);
            if (user.IsPoPulated()) {
                UserSetupEmailService.SendActivationEmail(user, email);
            }

        }

        public bool VerifyChangePassword(InMemoryUser user) {
            return !_ldapManager.IsLdapSetup() && user.ChangePassword;
        }
    }
}
