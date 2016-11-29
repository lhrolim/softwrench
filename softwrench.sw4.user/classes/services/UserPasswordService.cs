using System;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.Util;
using JetBrains.Annotations;
using softwrench.sw4.api.classes.configuration;
using softwrench.sw4.user.classes.config;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.exceptions;

namespace softwrench.sw4.user.classes.services {

    public class UserPasswordService : ISingletonComponent {

        private readonly IConfigurationFacadeCommons _configFacade;
        private readonly ISWDBHibernateDAO _dao;

        public UserPasswordService(IConfigurationFacadeCommons configFacade, ISWDBHibernateDAO dao) {
            _configFacade = configFacade;
            _dao = dao;
        }


        /// <summary>
        /// Checks whether the password match and also perform password lock policy handling
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="password"></param>
        /// <returns></returns>

        public async Task<bool> MatchPassword(User dbUser, string password) {
            var encryptedPassword = AuthUtils.GetSha1HashData(password);



            var maxAttempts = await _configFacade.LookupAsync<int>(UserConfigurationConstants.WrongPasswordAttempts);
            var matchPassword = dbUser.Password.Equals(encryptedPassword);
            if (maxAttempts <= 0) {
                if (dbUser.AuthenticationAttempts != null && !dbUser.AuthenticationAttempts.IsPristine()) {

                    //restoring state
                    dbUser.AuthenticationAttempts.Clear();
                    await _dao.SaveAsync(dbUser.AuthenticationAttempts);
                    if (dbUser.Locked.HasValue && dbUser.Locked.Value) {
                        dbUser.Locked = false;
                        await _dao.SaveAsync(dbUser);
                    }
                }
                return matchPassword;
            }



            var attempts = dbUser.AuthenticationAttempts;

            if (attempts == null) {
                attempts = await _dao.FindSingleByQueryAsync<AuthenticationAttempt>(AuthenticationAttempt.ByUser, dbUser.Id);
                if (attempts == null) {
                    dbUser.AuthenticationAttempts = new AuthenticationAttempt();
                } else {
                    dbUser.AuthenticationAttempts = attempts;
                }
                attempts = dbUser.AuthenticationAttempts;
            }

            attempts.UserId = dbUser.Id.Value;

            var isLocked = dbUser.Locked.HasValue && dbUser.Locked.Value;
            if (matchPassword) {

                if (!isLocked) {
                    //cleaning up
                    attempts.Clear();
                }
            } else {
                attempts.GlobalNumberOfAttempts++;
                attempts.NumberOfAttempts++;
                attempts.RegisterTime = DateTime.Now;
                if (attempts.NumberOfAttempts > maxAttempts || attempts.GlobalNumberOfAttempts > maxAttempts * 3) {
                    dbUser.Locked = true;
                    await _dao.SaveAsync(dbUser);
                } else if (dbUser.Locked.HasValue && dbUser.Locked.Value) {
                    dbUser.Locked = false;
                    await _dao.SaveAsync(dbUser);
                }
            }

            await _dao.SaveAsync(attempts);

            return matchPassword;
        }


        /// <summary>
        /// Checks whether the given password can be used according to the UserConfigurationConstants.MinPasswordHistorySize configuration policy, creating a history entry.
        /// 
        /// Also, updates the password expiration time of the current user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="criptedPassword"></param>
        /// <exception cref="PasswordException"> case the password was repeated according to the current system policy</exception>
        /// <returns>true if the password matches the current system policy false otherwise</returns>
        public virtual async Task HandlePasswordHistory([NotNull]User user, [NotNull] string criptedPassword) {
            if (user.Id == null) {
                //user creation
                return;
            }

            var length = await _configFacade.LookupAsync<int>(UserConfigurationConstants.MinPasswordHistorySize);

            if (length <= 0) {
                await DoUpdatePasswordExpiration(user);
                return;
            }

            var passwordHistories =
                await _dao.FindByQueryWithLimitAsync<PasswordHistory>(PasswordHistory.ByUserDesc, length, user.Id);
            var anyMatching = passwordHistories.Any(p => p.Password.Equals(criptedPassword));
            if (anyMatching) {
                throw PasswordException.HistoryException(length);
            }


            await _dao.SaveAsync(new PasswordHistory {
                UserId = user.Id.Value,
                RegisterTime = DateTime.Now,
                Password = criptedPassword
            });
            await DoUpdatePasswordExpiration(user);

        }

        private async Task DoUpdatePasswordExpiration(User user) {
            var passwordExpirationDays = await _configFacade.LookupAsync<int>(UserConfigurationConstants.PasswordExpirationTime);
            var now = DateTime.Now;
            if (passwordExpirationDays > 0) {
                var futureDate = now.AddDays(passwordExpirationDays);
                user.PasswordExpirationTime = futureDate;
                user.ChangePassword = false;
                await _dao.SaveAsync(user);
            }
        }

        /// <summary>
        /// To be called on user creation workflow, since there´s "no way" to check the history and set the history before the id exists
        /// </summary>
        /// <param name="user"></param>
        /// <param name="criptedPassword"></param>
        /// <returns></returns>
        public virtual async Task SetFirstPasswordHistory(User user, [NotNull] string criptedPassword) {
            
            if (user?.Id == null) {
                throw new InvalidOperationException("user cannot be null or transient");
            }

            await _dao.SaveAsync(new PasswordHistory {
                UserId = user.Id.Value,
                RegisterTime = DateTime.Now,
                Password = criptedPassword
            });

            await DoUpdatePasswordExpiration(user);
        }
    }
}
