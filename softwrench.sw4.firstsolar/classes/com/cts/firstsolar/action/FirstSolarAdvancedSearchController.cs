using System.Collections.Generic;
using System.Web.Http;
using cts.commons.web.Attributes;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action {

    [Authorize]
    [SWControllerConfiguration]
    public class FirstSolarAdvancedSearchController : ApiController {

        private readonly FirstSolarAdvancedSearchHandler _advancedSearchHandler;

        public FirstSolarAdvancedSearchController(FirstSolarAdvancedSearchHandler advancedSearchHandler) {
            _advancedSearchHandler = advancedSearchHandler;
        }

        [HttpGet]
        public List<Dictionary<string, string>> GetLocationsOfInterest([FromUri] List<string> facilities) {
            return _advancedSearchHandler.GetLocationsOfInterest(facilities);
        }

        [HttpGet]
        public List<Dictionary<string, string>> GetSwitchgearLocations([FromUri] List<string> facilities) {
            return _advancedSearchHandler.GetSwitchgearLocations(facilities);
        }
    }
}
