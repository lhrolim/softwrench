using System.Web.Mvc;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Web.Models.User;

namespace softWrench.sW4.Web.Controllers.MyProfile
{
    public class MyProfileController : Controller
    {
        public ActionResult MyProfile()
        {
            var model = new UserModel(SecurityFacade.CurrentUser());
            return View(model);
        }
    }
}
