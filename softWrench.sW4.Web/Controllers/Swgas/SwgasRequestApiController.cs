using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.persistence;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Controllers.Swgas {
    public class SwgasRequestApiController : ApiController {

        private const string Client = "swgas";

        [Import]
        public DataController DataController { get; set; }

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

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

        [HttpGet]
        public async Task<IEnumerable<IAssociationOption>> LoadCities(string division) {
            var result = new List<AssociationOption>();
            var list = await Dao.FindByNativeQueryAsync("select distinct(city) from swg_division where division = ?", division);
            foreach (var item in list) {
                result.Add(new AssociationOption(item["city"], item["city"]));
            }
            return result;
        }

        [HttpGet]
        public async Task<IEnumerable<IAssociationOption>> LoadBuildings(string division, string city) {
            var result = new List<AssociationOption>();
            var list = await Dao.FindByNativeQueryAsync("select distinct(building) from swg_division where division = ? and city=?", division,city);
            foreach (var item in list) {
                result.Add(new AssociationOption(item["building"], item["building"]));
            }
            result.Add(new AssociationOption(city + " - " + "Other", city + " - " + "Other"));
            return result;
        }
    }
}
