﻿using System;
using cts.commons.portable.Util;

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

        public static Exception BatchReportNotFound(object batchId) {
            return new BatchException("batch report not found for batch {0}".Fmt(batchId));
        }

        public static Exception UnauthorizedException() {
            return new BatchException("This batch is not authorized for your user");
        }
    }
}
