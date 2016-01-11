using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.web.Attributes;
using log4net;
using log4net.Core;
using softWrench.sW4.SPF;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action {


    [Authorize]
    [SWControllerConfiguration]
    public class FirstSolarWorkorderBatchController : ApiController {

        private static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarWorkorderBatchController));

        public FirstSolarWorkorderBatchController() {
            Log.Debug("init log...");
        }


        [HttpPost]
        public void InitLocationBatch(BatchData batchData) {
            Log.Debug("receiving batch data");
        }

        public class BatchData {

            public string Summary {
                get; set;
            }
            public string Details {
                get; set;
            }
            public string SiteId {
                get; set;
            }
            public List<string> Locations {
                get; set;
            }
            public List<string> Assets {
                get; set;
            }

        }

    }
}
