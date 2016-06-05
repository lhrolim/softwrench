using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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


        private readonly UserLinkManager _userLinkManager;

        private readonly MaximoHibernateDAO _maxDAO;

        private readonly UserSetupEmailService _userSetupEmailService;

        private static LdapManager _ldapManager;

        public UserManager(UserLinkManager userLinkManager, MaximoHibernateDAO maxDAO, UserSetupEmailService userSetupEmailService, LdapManager ldapManager) {
            _userLinkManager = userLinkManager;
            _maxDAO = maxDAO;
            _userSetupEmailService = userSetupEmailService;
            _ldapManager = ldapManager;
        }


        private static IEventDispatcher _dispatcher;


        public static IEventDispatcher Dispatcher {
            get {
                if (_dispatcher == null) {
                    _dispatcher = SimpleInjectorGenericFactory.Instance.GetObject<IEventDispatcher>(typeof(IEventDispatcher));
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
            if (user.Id == null && user.MaximoPersonId == null) {
                user.MaximoPersonId = user.UserName;
            }
            if (updateMaximo) {
                user.Person.Save();
            }
            var savedUser = DAO.Save(user);
            // ReSharper disable once PossibleInvalidOperationException --> just saved
            Dispatcher.Dispatch(new UserSavedEvent(savedUser.Id.Value, savedUser.UserName));
            return savedUser;
        }

        public void ActivateAndDefinePassword(User user, string password) {
            user.Password = AuthUtils.GetSha1HashData(password);
            user.IsActive = true;
            user.ChangePassword = false;
            var savedUser = DAO.Save(user);
            //TODO: wipeout link
            _userLinkManager.DeleteLink(savedUser);
            // ReSharper disable once PossibleInvalidOperationException
            Dispatcher.Dispatch(new UserSavedEvent(savedUser.Id.Value, savedUser.UserName));
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
            var user = UserSyncManager.GetUserFromMaximoByUserName(personid, null,true);
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
            user.CustomRoles = new HashedSet<UserCustomRole>();
            if (save) {
                try {
                    return DAO.Save(user);
                } catch (Exception e) {
                    if (_ldapManager.IsLdapSetup()) {
                        //recovering from scenarios where there might have been an already created user on softwrench with a personid equal to an existing user in maximo
                        var legacyUser = DAO.FindSingleByQuery<User>(User.UserByUserNameOrPerson, user.UserName,user.MaximoPersonId);
                        if (legacyUser != null) {
                            legacyUser.UserName = user.UserName;
                            legacyUser.MaximoPersonId = user.MaximoPersonId;
                            return DAO.Save(legacyUser);
                        }
                    }
                    throw;
                }
            }

            return user;
        }

        public static User SyncLdapUser(User existingUser, bool isLdapSetup) {
            if (existingUser.MaximoPersonId == null || existingUser.Systemuser) {
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
            get {
                return ApplicationConfiguration.ClientName == "hapag" && ApplicationConfiguration.IsProd();
            }
        }

        public User FindUserByLink(string tokenLink, out bool hasExpired) {
            var user = _userLinkManager.RetrieveUserByLink(tokenLink, out hasExpired);
            if (user != null) {
                var maximoUser = UserSyncManager.GetUserFromMaximoByUserName(user.UserName, user.Id);
                user.MergeMaximoWithNewUser(maximoUser);
            }
            return user;
        }

        public string ForgotPassword(string userNameOrEmail) {
            var isEmail = new EmailAddressAttribute().IsValid(userNameOrEmail);
            User user;
            string emailToSend = null;
            if (!isEmail) {
                user = DAO.FindSingleByQuery<User>(User.UserByUserName, userNameOrEmail);
                if (user == null) {
                    return "User {0} not found".Fmt(userNameOrEmail);
                }
                var email = _maxDAO.FindSingleByNativeQuery<string>("select emailaddress from email where personid = ? and isPrimary = 1", user.MaximoPersonId);
                if (email == null) {
                    return "User {0} has no primary email registered. Please contact your administrator".Fmt(userNameOrEmail);
                }
                emailToSend = email;
            } else {
                emailToSend = userNameOrEmail;
                var personid = _maxDAO.FindSingleByNativeQuery<string>("select personid from email where emailaddress = ? and isPrimary = 1", userNameOrEmail);
                if (personid == null) {
                    return "The email {0} is not registered on the database".Fmt(userNameOrEmail);
                }
                user = DAO.FindSingleByQuery<User>(User.UserByMaximoPersonId, personid);
                if (user == null) {
                    return "The email {0} is not registered on the database".Fmt(userNameOrEmail);
                }
            }
            user = UserSyncManager.GetUserFromMaximoByUserName(user.UserName, user.Id);
            _userSetupEmailService.ForgotPasswordEmail(user, emailToSend);
            return null;
        }

        public void SendActivationEmail(int userId, string email) {
            var user = DAO.FindByPK<User>(typeof(User), userId);
            if (user == null) {
                throw new InvalidOperationException("user {0} not found".Fmt(userId));
            }
            user = UserSyncManager.GetUserFromMaximoByUserName(user.UserName, user.Id);
            _userSetupEmailService.SendActivationEmail(user, email);
        }

        public bool VerifyChangePassword(InMemoryUser user) {
            return !_ldapManager.IsLdapSetup() && user.ChangePassword;
        }
    }
}
