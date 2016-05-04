using System.Web.Http;
using cts.commons.web.Attributes;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata;

namespace softWrench.sW4.Web.Controllers {

    [Authorize]
    [SWControllerConfiguration]
    public class MetadataController : ApiController {

        [HttpGet]
        public ApplicationSchemaDefinition GetSchemaDefinition([FromUri] string applicationName, string targetSchemaId) {
            var app = MetadataProvider.Application(applicationName);
            var schema = app.Schema(new ApplicationMetadataSchemaKey(targetSchemaId));
            return schema;
        }
    }
}
