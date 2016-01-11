using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.web.Attributes;
using JetBrains.Annotations;
using log4net;
using log4net.Core;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.SPF;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action {


    [Authorize]
    [SPFRedirect(URL = "Application")]
    [SWControllerConfiguration]
    public class FirstSolarWorkorderBatchController : ApiController {

        private static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarWorkorderBatchController));

        private MaximoHibernateDAO _maxDAO;

        public FirstSolarWorkorderBatchController(MaximoHibernateDAO maxDAO) {
            _maxDAO = maxDAO;
            Log.Debug("init log...");
        }


        [HttpPost]
        public ApplicationListResult InitLocationBatch(BatchData batchData) {

            Log.Debug("receiving batch data");
            var warningIds = ValidateIdsThatHaveWorkorders(batchData.Locations, true);

            var resultData = batchData.Locations.Select(location => GetDataMap(location, batchData, warningIds)).ToList();
            var schema = MetadataProvider.Application("workorder").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("batchLocationSpreadSheet")).Schema;

            return new ApplicationListResult(batchData.Locations.Count, null, resultData, schema, null);



    }

    private DataMap GetDataMap(IAssociationOption location, BatchData batchData, ICollection<string> warningIds) {
        var selected = !warningIds.Contains(location.Value);
        var fields = new Dictionary<string, object>();
        fields["_#selected"] = selected;
        fields["summary"] = batchData.Summary;
        fields["siteid"] = batchData.SiteId;
        fields["details"] = batchData.Details;
        fields["location_label"] = location.Label;
        fields["location"] = location.Value;
        return new DataMap("workorder", fields);
    }

    [NotNull]
    public List<string> ValidateIdsThatHaveWorkorders(List<AssociationOption> originalIds, bool location) {
        return new List<string>();
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
        public List<AssociationOption> Locations {
            get; set;
        }
        public List<AssociationOption> Assets {
            get; set;
        }

    }

}
}
