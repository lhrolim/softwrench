using System;

namespace softWrench.sW4.Metadata.Applications.DataSet.baseclasses {
    public class ApplicationFiltereableKey {
        readonly string _application;
        readonly string _schemaId;
        readonly string _client;

        public ApplicationFiltereableKey(string application, string client, string schemaId) {
            _application = application.ToLower();
            _client = client;
            _schemaId = schemaId;
        }

        private bool Equals(ApplicationFiltereableKey other) {
            var applicationEquals = string.Equals(_application, other._application, StringComparison.CurrentCultureIgnoreCase);
            var clientEquals = _client == null || string.Equals(_client, other._client, StringComparison.CurrentCultureIgnoreCase);
            var schemaEquals = _schemaId == null || string.Equals(_schemaId, other._schemaId, StringComparison.CurrentCultureIgnoreCase);
            return applicationEquals && clientEquals && schemaEquals;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationFiltereableKey)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((_application != null ? _application.ToLower().GetHashCode() : 0) * 397) ^ (_client != null ? _client.ToLower().GetHashCode() : 0) ^ (_schemaId != null ? _schemaId.ToLower().GetHashCode() : 0);
            }
        }

        public override string ToString() {
            return string.Format("Application: {0}, Client: {1}", _application, _client);
        }
    }
}
