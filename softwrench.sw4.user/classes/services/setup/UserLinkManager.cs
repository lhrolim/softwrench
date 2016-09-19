using System;
using System.Globalization;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.Util;
using log4net;
using softwrench.sw4.user.classes.entities;

namespace softwrench.sw4.user.classes.services.setup {
    public class UserLinkManager : ISingletonComponent {

        //Todo make a configuration --> requires more modularization
        private const int DaysToAdd = 10;

        private readonly ISWDBHibernateDAO _dao;

        private readonly ILog _log = LogManager.GetLogger(typeof(UserLinkManager));


        public UserLinkManager(ISWDBHibernateDAO dao) {
            _log.Debug("init log");
            _dao = dao;
        }

        public async Task<UserActivationLink> GetLinkByUser(User user) {
            var existingLink = await _dao.FindSingleByQueryAsync<UserActivationLink>(UserActivationLink.TokenByUser, user);
            return existingLink;
        }

        public string GenerateTokenLink(User user) {
            var existingLink = _dao.FindSingleByQuery<UserActivationLink>(UserActivationLink.TokenByUser, user);
            if (existingLink == null) {
                return DoGenerateNewLink(user);
            }
            if (existingLink.HasExpired()) {
                DoDeleteLink(existingLink);
                return DoGenerateNewLink(user);
            }
            _log.InfoFormat("updating expiration date and returning previous existing token for user {0}", user.UserName);
            existingLink.ExpirationDate = DateUtil.EndOfDay(DateTime.Now.AddDays(DaysToAdd));
            _dao.Save(existingLink);
            return existingLink.Token;
        }

        private string DoGenerateNewLink(User user) {

            var token = "" + new Random(100).Next(10000);
            token += DateTime.Now.TimeInMillis().ToString(CultureInfo.InvariantCulture);
            token += AuthUtils.GetSha1HashData(token);

            var link = new UserActivationLink {
                User = user,
                ExpirationDate = DateUtil.EndOfDay(DateTime.Now.AddDays(DaysToAdd)),
                Token = token,
                SentDate = DateTime.Now
            };

            _dao.Save(link);

            return token;

        }

        public User RetrieveUserByLink(string link, out bool hasExpired) {
            var activationLink = _dao.FindSingleByQuery<UserActivationLink>(UserActivationLink.UserByToken, link);
            hasExpired = false;
            if (activationLink == null) {
                return null;
            }
            if (activationLink.HasExpired()) {
                DoDeleteLink(activationLink);
                hasExpired = true;
                return null;
            }

            return activationLink.User;
        }

        private void DoDeleteLink(UserActivationLink activationLink) {
            _log.InfoFormat("deleting link that has expired for user {0}", activationLink.User.UserName);
            _dao.Delete(activationLink);
        }


        public void DeleteLink(User user) {
            _dao.ExecuteSql("delete from USER_ACTIVATIONLINK where user_id = ?", user.Id);
        }
    }
}
