using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Batches;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dispatch {

    public class WorkorderFromDispatchConverter : ABatchSubmissionConverter {
        public static string BatchOperationName = "create_fromdispatch";
        public static string BatchPropertyKey = "_#inverterwos";

        public override string ApplicationName() {
            return "workorder";
        }

        public override string ClientFilter() {
            return "firstsolar";
        }

        public override string SchemaId() {
            return BatchOperationName;
        }

        public override string BatchProperty => BatchPropertyKey;
    }
}
