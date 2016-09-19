using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace softWrench.sW4.Web.Controllers {

    [System.Web.Mvc.Authorize]
    public class ReportFilterController : ApiController {

        private readonly DataController _dataController;
        private readonly IConfigurationFacade _configFacade;
        private IWhereClauseFacade _whereClauseFacade;
        private readonly I18NResolver _i18Nresolver;


        public ReportFilterController(DataController dataController, IConfigurationFacade configFacade, IWhereClauseFacade whereClauseFacade, I18NResolver i18Nresolver) {
            _dataController = dataController;
            _configFacade = configFacade;
            _whereClauseFacade = whereClauseFacade;
            _i18Nresolver = i18Nresolver;
        }

        public async Task<IApplicationResponse> Get(string application, [FromUri] DataRequestAdapter request) {
            var user = SecurityFacade.CurrentUser();

            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var response = await _dataController.Get(application, request);

            return response;
        }
    }
}
