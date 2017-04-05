using System;

namespace softwrench.sw4.api.classes.application {
    public class ApplicationFiltereableKey {
        readonly string _application;
        readonly string _extraKey;
        readonly string _client;

        public ApplicationFiltereableKey(string application, string client, string extraKey) {
            _application = application == null ? null : application.ToLower();
            _client = client;
            _extraKey = extraKey;
        }

        private bool Equals(ApplicationFiltereableKey other) {
            var applicationEquals = string.Equals(_application, other._application, StringComparison.CurrentCultureIgnoreCase);
            var clientEquals = _client == null || string.Equals(_client, other._client, StringComparison.CurrentCultureIgnoreCase);
            var extraKeyEquals = _extraKey == null || string.Equals(_extraKey, other._extraKey, StringComparison.CurrentCultureIgnoreCase);
            return applicationEquals && clientEquals && extraKeyEquals;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationFiltereableKey)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((_application?.ToLower().GetHashCode() ?? 0) * 397) ^ (_client?.ToLower().GetHashCode() ?? 0) ^ (_extraKey?.ToLower().GetHashCode() ?? 0);
            }
        }

        public override string ToString() {
            return $"Application: {_application}, Client: {_client}";
        }
    }
}
