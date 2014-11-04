using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission {

    //TODO: make this specific to TVA, and modify SimpleInjector
    public class WorkorderSubmissionConverter : ISubmissionConverter {
        public bool ShouldSubmit(JObject row) {
            return "true".EqualsIc(row.StringValue("#closed"));
        }

        public CrudOperationData Convert(JObject row) {
            var completeApp = MetadataProvider.Application("workorder");
            var app = completeApp.ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("detail"));
            var entityMetadata = MetadataProvider.Entity("workorder");
            var id = row.StringValue("wonum");
            return new CrudOperationData(id, BuildAttributes(row), new Dictionary<string, object>(), entityMetadata, app);
        }

        private static Dictionary<string, object> BuildAttributes(JObject row) {
            return new Dictionary<string, object>();
        }

        public string ApplicationName() {
            return "workorder";
        }
    }
}
