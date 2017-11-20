using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Web.Http;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Controllers;

namespace softwrench.sw4.web.Controllers.Umc {
    public class UmcRequestApiController : ApiController {

        private const string Client = "umc";

        [Import]
        public DataController DataController { get; set; }

        [HttpPost]
        public async Task<IApplicationResponse> Submit([FromBody]JsonRequestWrapper wrapper) {
            if (!Client.Equals(ApplicationConfiguration.ClientName)) {
                return null;
            }
            var response = await DataController.Post(wrapper) as ApplicationDetailResult;
            return new BlankApplicationResponse() {
                Id = response?.ResultObject.GetStringAttribute("ticketid")
            };
        }
    }
}
