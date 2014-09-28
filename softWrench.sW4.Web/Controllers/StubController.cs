using System.Web.Mvc;
using System.Web.Security;
using softWrench.sW4.Metadata;

namespace softWrench.sW4.Web.Controllers
{
    public class StubController : Controller
    {
        public ActionResult Reset()
        {
            MetadataProvider.StubReset();
            FormsAuthentication.SignOut();
            return Redirect("~");
        }

    }
}
