
using System.Security;
using System.Threading;
using System.Web.Mvc;
using System.Web.Security;
using cts.commons.persistence;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Common;
using softWrench.sW4.Web.Models.UserSetup;

namespace softWrench.sW4.Web.Controllers.Security {

    /// <summary>
    /// A user controller which contains methods specifically oriented for setup, that do not require any authentication.
    /// </summary>
    public class UserSetupController : Controller {
        private const string WrongLinkException = "the link you´re trying to access is not valid";

        private readonly SecurityFacade _facade = SecurityFacade;
        private static readonly SecurityFacade SecurityFacade = SecurityFacade.GetInstance();

        private readonly ISWDBHibernateDAO _dao;
        private readonly UserManager _userManager;

        public UserSetupController(ISWDBHibernateDAO dao, UserManager userManager) {
            this._dao = dao;
            _userManager = userManager;
        }

        [System.Web.Http.HttpGet]
        public ActionResult DefinePassword(string tokenLink) {
            Validate.NotNull(tokenLink, "tokenLink");
            var user = _userManager.FindUserByLink(tokenLink);
            if (user == null) {
                throw new SecurityException(WrongLinkException);
            }
            return View(new DefinePasswordModel {
                Token = tokenLink,
                FirstName = user.FirstName,
                LastName = user.LastName
            });
        }

        [System.Web.Http.HttpPost]
        [SPFRedirect(URL = "DefinePassword")]
        public ActionResult DoSetPassword(string tokenLink, string password, string userTimezoneOffset) {
            Validate.NotNull(tokenLink, "tokenLink");
            Validate.NotNull(password, "password");
            var user = _userManager.FindUserByLink(tokenLink);
            if (user == null) {
                throw new SecurityException(WrongLinkException);
            }
            _userManager.DefineUserPassword(user, password);
            
            //logining in the user and redirecting him to home page
            var inMemoryUser = SecurityFacade.GetInstance().Login(user, password, userTimezoneOffset);
            AuthenticationCookie.SetSessionCookie(user.UserName, userTimezoneOffset, Response);
            FormsAuthentication.RedirectFromLoginPage(user.UserName, false);
            Thread.CurrentPrincipal = inMemoryUser;

            return null;
        }


    }
}