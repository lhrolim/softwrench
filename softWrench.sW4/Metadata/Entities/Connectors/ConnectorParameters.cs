using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Entities.Connectors {
    public class ConnectorParameters {
        private readonly IDictionary<string, string> _parameters;

        public const string UpdateInterfaceParam = "integration_interface";

        public Boolean ExcludeUndeclared { get; set; }

        public ConnectorParameters([NotNull] IDictionary<string, string> parameters, Boolean excludeUndeclared) {
            if (parameters == null) throw new ArgumentNullException("parameters");

            _parameters = parameters;
            ExcludeUndeclared = excludeUndeclared;
        }

        [NotNull]
        public IDictionary<string, string> Parameters {
            get { return _parameters; }
        }

        public static ConnectorParameters DefaultInstance() {
            return new ConnectorParameters(new Dictionary<string, string>(), false);
        }

        public string GetWSEntityKey(string keyToUse = ConnectorParameters.UpdateInterfaceParam) {
            string entityKey;
            if (!_parameters.TryGetValue(ApplicationConfiguration.WsProvider + "_" + keyToUse, out entityKey)) {
                _parameters.TryGetValue(keyToUse, out entityKey);
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
