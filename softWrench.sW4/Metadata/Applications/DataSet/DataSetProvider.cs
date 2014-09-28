using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.DataSet {

    public class DataSetProvider {

        private static DataSetProvider _instance;

        private readonly BaseApplicationDataSet _defaultSet = new BaseApplicationDataSet();

        private readonly IDictionary<DataSetKey, IDataSet> _dataSets = new Dictionary<DataSetKey, IDataSet>();

        private DataSetProvider() {
            var assemblies = AssemblyLocator.GetSWAssemblies();
            var dataSets = new List<Type>();
            foreach (var assembly in assemblies) {
                dataSets.AddRange(assembly.GetTypes().Where(type => typeof(IDataSet).IsAssignableFrom(type)));
            }

            foreach (var dataSetType in dataSets) {
                if (dataSetType.IsInterface) {
                    continue;
                }
                var dataSet = (IDataSet)Activator.CreateInstance(dataSetType);
                string applicationName = dataSet.ApplicationName();
                if (applicationName == null) {
                    //null stands for BaseApplicationDataSet only
                    continue;
                }
                var clientFilter = dataSet.ClientFilter();
                if (clientFilter != null) {
                    var strings = clientFilter.Split(',');
                    foreach (var client in strings) {
                        _dataSets.Add(new DataSetKey(applicationName, client), dataSet);
                    }
                } else {
                    _dataSets.Add(new DataSetKey(applicationName, null), dataSet);
                }
            }
        }

        public IDataSet LookupDataSet(String applicationName) {
            var key = new DataSetKey(applicationName, ApplicationConfiguration.ClientName);
            if (!_dataSets.ContainsKey(key)) {
                key = new DataSetKey(applicationName, null);
            }
            return _dataSets.ContainsKey(key) ? _dataSets[key] : _defaultSet;
        }

        public BaseApplicationDataSet LookupAsBaseDataSet(String applicationName) {
            var key = new DataSetKey(applicationName, ApplicationConfiguration.ClientName);
            var dataSet = LookupDataSet(applicationName);
            var resultSet = dataSet as BaseApplicationDataSet;
            return resultSet ?? _defaultSet;
        }

        public static DataSetProvider GetInstance() {
            return _instance ?? (_instance = new DataSetProvider());
        }

        class DataSetKey {
            readonly string _application;
            readonly string _client;

            public DataSetKey(string application, string client) {
                _application = application;
                _client = client;
            }

            private bool Equals(DataSetKey other) {
                var applicationEquals = string.Equals(_application, other._application);
                var clientEquals = _client == null || string.Equals(_client, other._client);
                return applicationEquals && clientEquals;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((DataSetKey)obj);
            }

            public override int GetHashCode() {
                unchecked {
                    return ((_application != null ? _application.GetHashCode() : 0) * 397) ^ (_client != null ? _client.GetHashCode() : 0);
                }
            }
        }

    }
}
