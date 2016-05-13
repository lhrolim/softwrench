using System;
using softWrench.sW4.Security.Services;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web;

namespace softWrench.sW4.Security.Attributes {

    /// <summary>
    /// Attribute class to check for dynamic role for user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class DynamicAdminRoleAttribute : AuthorizeAttribute {

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicAdminRoleAttribute"/> class.
        /// </summary>
        public DynamicAdminRoleAttribute() {
        }

        /// <summary>
        /// Verifies that the current user has sufficient role for authorization.
        /// </summary>
        /// <param name="actionContext">the action context</param>
        /// <returns>The authorization status</returns>
        protected override bool IsAuthorized(HttpActionContext actionContext) {
            var user = SecurityFacade.CurrentUser();

            if(user != null) {
                return user.IsInRole("dynamicadmin");
            }

            return false;
        }

        /// <summary>
        /// The handle unauthorized request.
        /// </summary>
        /// <param name="actionContext">The filter context.</param>
        /// <exception cref="HttpException">Unauthorized access exception</exception>
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext) {
            throw new HttpException(401, "Unauthorized Access");
        }
    }
}