using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softWrench.sW4.Data.Persistence.Operation;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services {
    internal class BatchSubmissionItem {

        internal OperationWrapper CrudData { get; set; }
        internal JObject OriginalLine { get; set; }

        /// <summary>
        /// when there´s one persistent item available
        /// </summary>
        internal BatchItem OriginalItem { get; set; }
    }
}
