using System.Web.Mvc;
using softwrench.sw4.webcommons.classes.api;
using softWrench.sW4.Web.Models.Home;
using static softWrench.sW4.Web.Controllers.ErrorController;

namespace softWrench.sW4.Web.Controllers {
    [Authorize]
    [NoMenuController]
    public class NoMenuErrorController : Controller {

        private readonly HomeService _homeService;

        public NoMenuErrorController(HomeService homeService) {
            _homeService = homeService;
        }

        public ActionResult ErrorFallback() {
            return View(ErrorIndex, ErrorModel(_homeService, Request));
        }
    }
}