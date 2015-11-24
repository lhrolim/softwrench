using System.Configuration;
using cts.commons.portable.Util;
using softWrench.sW4.Exceptions;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.exception {
    public class BatchConfigurationException : MetadataException {

        public BatchConfigurationException(string message)
            : base(message) {

        }

        public static BatchConfigurationException BatchNotFound(string application, string schema, string client) {
            return new BatchConfigurationException("Batch Converter not found for application: {0} schema: {1} client: {2}".Fmt(application, schema, client));
        }


    }
}
