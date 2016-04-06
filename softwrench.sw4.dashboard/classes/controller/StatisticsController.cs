using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.web.Attributes;
using softwrench.sw4.dashboard.classes.service.statistics;
using softWrench.sW4.SPF;

namespace softwrench.sw4.dashboard.classes.controller {
    [Authorize]
    [SPFRedirect(URL = "Application")]
    [SWControllerConfiguration]
    public class StatisticsController : ApiController {

        private readonly StatisticsService _service;

        public StatisticsController(StatisticsService service) {
            _service = service;
        }

        [HttpGet]
        public async Task<IEnumerable<StatisticsResponseEntry>> CountByProperty([FromUri]StatisticsRequest request) {
            return await _service.CountByProperty(request);
        }

    }
}
