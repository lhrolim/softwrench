using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Security.Services;
using System;
using System.Web;
using System.Web.Mvc;

namespace softWrench.sW4.Security.Attributes {

    /// <summary>
    /// Attribute class to check for system admin role for user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SwAdminAuthorizedMvcAttribute : AuthorizeAttribute {

        /// <summary>
        /// Initializes a new instance of the <see cref="SwAdminAuthorizedMvcAttribute"/> class.
        /// </summary>
        public SwAdminAuthorizedMvcAttribute() {
        }

        /// <summary>
        /// Verifies that the current user has sufficient role for authorization.
        /// </summary>
        public override void OnAuthorization(AuthorizationContext filterContext) {
            var user = filterContext.HttpContext.User;

            if (user == null || !user.IsInRole(Role.SysAdmin)) {
                throw new HttpException(401, "Unauthorized Access");
            }
        }
    }
}