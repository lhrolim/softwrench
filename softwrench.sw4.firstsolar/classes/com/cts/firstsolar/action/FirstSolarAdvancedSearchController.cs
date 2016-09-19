using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.web.Attributes;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset.advancedsearch;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action {

    [Authorize]
    [SWControllerConfiguration]
    public class FirstSolarAdvancedSearchController : ApiController {

        private readonly FirstSolarAdvancedSearchHandler _advancedSearchHandler;
        private DataSetProvider _dataSetProvider;

        public FirstSolarAdvancedSearchController(FirstSolarAdvancedSearchHandler advancedSearchHandler, DataSetProvider dataSetProvider) {
            _advancedSearchHandler = advancedSearchHandler;
            _dataSetProvider = dataSetProvider;
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

        [HttpGet]
        public async Task<ApplicationListResult> FindAssetsBySelectedLocations(bool includeSubLocations, [FromUri] List<LocationDTO> locations) {

            var app = MetadataProvider.Application("asset").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("assetLookupList"));
            var dto = new PaginatedSearchRequestDto();
//            dto.WhereClause =_advancedSearchHandler.BuildAdvancedSearchWhereClause(locations.Select(s => s.LocationKey).ToList(),"asset", includeSubLocations);
            dto.FilterFixedWhereClause = _advancedSearchHandler.BuildAdvancedSearchWhereClause(locations.Select(s => s.Location).ToList(), "asset", includeSubLocations);
            var dataSet = _dataSetProvider.LookupDataSet("asset", "assetLookupList");
            var result = await dataSet.GetList(app, dto);
            return result;
        }


        public class LocationDTO {
            public string Location {
                get; set;
            }
            public string SiteId {
                get; set;
            }
        }


    }
}
