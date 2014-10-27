using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.exception {
    public class BatchException : InvalidOperationException {

        public BatchException(string message)
            : base(message) {

        }

        public static BatchException BatchNotFound(object id) {
            return new BatchException("batch {0} not found".Fmt(id));
        }

        public static BatchException BatchIdNotInformed() {
            return new BatchException("batch id should be informed, contact your administrator");
        }
    }
}
