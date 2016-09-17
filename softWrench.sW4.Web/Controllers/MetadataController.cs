using System.Web.Http;
using cts.commons.web.Attributes;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata;
using softWrench.sW4.Web.Models.Home;

namespace softWrench.sW4.Web.Controllers {

    [Authorize]
    [SWControllerConfiguration]
    public class MetadataController : ApiController {

        private readonly RouteService _routeService;

        public MetadataController(RouteService routeService) {
            _routeService = routeService;
        }

        [HttpGet]
        public ApplicationSchemaDefinition GetSchemaDefinition([FromUri] string applicationName, string targetSchemaId) {
            var app = MetadataProvider.Application(applicationName);
            var schema = app.Schema(new ApplicationMetadataSchemaKey(targetSchemaId));
            return schema;
        }

        [HttpGet]
        public RouteInfo GetRouteInfo() {
            return _routeService.GetRouteInfo();
        }
    }
}
