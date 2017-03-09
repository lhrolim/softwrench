using System.Web;
using System.Web.Mvc;
using softWrench.sW4.Web.Models;
using softWrench.sW4.Web.Models.Home;

namespace softWrench.sW4.Web.Controllers {

    [Authorize]
    public class ErrorController : Controller {

        public const string ErrorIndex = "~/Views/Home/Index.cshtml";

        private readonly HomeService _homeService;

        public ErrorController(HomeService homeService) {
            _homeService = homeService;
        }

        public ActionResult ErrorFallback() {
            return View(ErrorIndex, ErrorModel(_homeService, Request));
        }

        public static HomeModel ErrorModel(HomeService homeService, HttpRequestBase request) {
            var model = homeService.BaseHomeModel(request, null);
            model.Error = ErrorConfig.GetLastError();
            model.Title = "Error | softWrench";
            return model;
        }
    }
}