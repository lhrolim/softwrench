using System.Collections.Generic;
using System.Web.Http;
using cts.commons.web.Attributes;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action {

    [Authorize]
    [SWControllerConfiguration]
    public class FirstSolarAdvancedSearchController : ApiController {

        private readonly FirstSolarPCSLocationHandler _pcsLocationHandler;

        public FirstSolarAdvancedSearchController(FirstSolarPCSLocationHandler pcsLocationHandler) {
            _pcsLocationHandler = pcsLocationHandler;
        }

        [HttpGet]
        public List<Dictionary<string, string>> GetLocationsOfInterest([FromUri] string facility) {
            return _pcsLocationHandler.GetLocationsOfInterest(facility);
        }

        [HttpGet]
        public List<Dictionary<string, string>> GetSwitchgearLocations([FromUri] string facility) {
            return _pcsLocationHandler.GetSwitchgearLocations(facility);
        }
    }
}
