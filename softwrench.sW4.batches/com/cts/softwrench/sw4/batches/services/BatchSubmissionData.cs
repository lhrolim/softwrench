using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services {
    class BatchSubmissionData {
        internal List<BatchSubmissionItem> ItemsToSubmit = new List<BatchSubmissionItem>();

        internal JArray RemainingArray = new JArray();
        internal ISet<string> RemainingIds = new HashSet<string>();

        internal void AddItem(BatchSubmissionItem item) {
            ItemsToSubmit.Add(item);
        }

        internal Boolean ShouldSubmit() {
            return ItemsToSubmit.Any();
        }
    }
}
