using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using softWrench.sW4.Data.Persistence.WS.Internal.Constants;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Entities.Connectors {
    public class ConnectorParameters {
        private readonly IDictionary<string, string> _parameters;

        public const string UpdateInterfaceParam = "integration_interface";

        public Boolean ExcludeUndeclared {
            get; set;
        }

        public ConnectorParameters([NotNull] IDictionary<string, string> parameters, Boolean excludeUndeclared) {
            if (parameters == null) throw new ArgumentNullException("parameters");

            _parameters = parameters;
            ExcludeUndeclared = excludeUndeclared;
        }

        [NotNull]
        public IDictionary<string, string> Parameters {
            get {
                return _parameters;
            }
        }

        public static ConnectorParameters DefaultInstance() {
            return new ConnectorParameters(new Dictionary<string, string>(), false);
        }

        public string GetWSEntityKey(string keyToUse = UpdateInterfaceParam, WsProvider? provider = null) {
            string entityKey;
            var providerName = ApplicationConfiguration.WsProvider;
            if (provider != null) {
                providerName = provider.ToString().ToLower();
            }
            if (!_parameters.TryGetValue(ApplicationConfiguration.Profile + "_" + providerName + "_" + keyToUse, out entityKey)) {
                if (!_parameters.TryGetValue(providerName + "_" + keyToUse, out entityKey)) {
                    _parameters.TryGetValue(keyToUse, out entityKey);
                }
            }
            if (providerName == "rest") {

            }

            return entityKey;
        }

        public void Merge(ConnectorParameters parameters) {
            if (parameters.ExcludeUndeclared) {
                _parameters.Clear();
            }

            foreach (var newParameter in parameters.Parameters) {
                if (_parameters.ContainsKey(newParameter.Key)) {
                    _parameters.Remove(newParameter.Key);
                }
                _parameters.Add(newParameter);
            }

        }
    }
}
