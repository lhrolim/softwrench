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

        /// <summary> 
        /// </summary>
        /// <param name="facilities">The list of selected facilities (right now one facility can represent a list).</param>
        /// <returns>The list of locations of interest.</returns>
        [HttpGet]
        public List<Dictionary<string, string>> GetLocationsOfInterest([FromUri] List<string> facilities) {
            return _advancedSearchHandler.GetLocationsOfInterest(facilities);
        }

        /// <summary> 
        /// </summary>
        /// <param name="facilities">The list of selected facilities (right now one facility can represent a list).</param>
        /// <returns>The list of switchgear locations.</returns>
        [HttpGet]
        public List<Dictionary<string, string>> GetSwitchgearLocations([FromUri] List<string> facilities) {
            return _advancedSearchHandler.GetSwitchgearLocations(facilities);
        }

        /// <summary> 
        /// </summary>
        /// <param name="facilities">The list of selected facilities (right now one facility can represent a list).</param>
        /// <returns>The list of pcs locations.</returns>
        [HttpGet]
        public List<Dictionary<string, string>> GetAvailablePcsLocations([FromUri] List<string> facilities) {
            return _advancedSearchHandler.GetAvailablePcsLocations(facilities);
        }
    }
}
