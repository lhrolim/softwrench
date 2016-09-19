using System.Collections.Generic;
using System.Web.Http;
using cts.commons.web.Attributes;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;
using softWrench.sW4.Util.DeployValidation;

namespace softWrench.sW4.Web.Controllers.Utilities {

    [Authorize]
    [SWControllerConfiguration]
    public class DeployValidationController : ApiController {

        private readonly DeployValidationService _deployValidationService;


        public DeployValidationController(DeployValidationService deployValidationService) {
            _deployValidationService = deployValidationService;
        }

        [HttpGet]
        [SPFRedirect("Deploy Validation", "_headermenu.deployvalidation", "DeployValidation")]
        public GenericResponseResult<Dictionary<string, Dictionary<string, object>>> Index() {
            var applications = _deployValidationService.GetAllApplicationInfo();
            return new GenericResponseResult<Dictionary<string, Dictionary<string, object>>>(applications);
        }

        [HttpGet]
        public GenericResponseResult<Dictionary<string, DeployValidationResult>> Validate() {
            var user = SecurityFacade.CurrentUser();
            var result = _deployValidationService.ValidateServices(user);
            return new GenericResponseResult<Dictionary<string, DeployValidationResult>>(result);
        }
    }
}
