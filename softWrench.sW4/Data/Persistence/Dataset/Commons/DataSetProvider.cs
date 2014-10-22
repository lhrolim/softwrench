using System;
using System.Collections.Generic;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.SimpleInjector.Events;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {

    public class DataSetProvider : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        private static DataSetProvider _instance;

        private readonly MaximoApplicationDataSet _defaultMaximoDataSet;
        private readonly MaximoApplicationDataSet _defaultSWDBDataSet;


        private readonly IDictionary<DataSetKey, IDataSet> _maximoDataSets = new Dictionary<DataSetKey, IDataSet>();
        private readonly IDictionary<DataSetKey, IDataSet> _swdbDataSets = new Dictionary<DataSetKey, IDataSet>();

        public DataSetProvider(MaximoApplicationDataSet defaultMaximoDataSet, MaximoApplicationDataSet defaultSWDBDataSet) {
            _defaultMaximoDataSet = defaultMaximoDataSet;
            _defaultSWDBDataSet = defaultSWDBDataSet;
        }


        private DataSetProvider() {
            //            var assemblies = AssemblyLocator.GetSWAssemblies();
            //            var dataSets = new List<Type>();
            //            foreach (var assembly in assemblies) {
            //                dataSets.AddRange(assembly.GetTypes().Where(type => typeof(IDataSet).IsAssignableFrom(type)));
            //            }

        }

        public IDataSet LookupDataSet(String applicationName) {
            var isSWDBApplication = applicationName.StartsWith("_");
            var defaultSet = isSWDBApplication ? _defaultSWDBDataSet : _defaultMaximoDataSet;
            var storageToUse = isSWDBApplication ? _swdbDataSets : _maximoDataSets;

            var key = new DataSetKey(applicationName.ToLower(), ApplicationConfiguration.ClientName);
            if (!storageToUse.ContainsKey(key)) {
                key = new DataSetKey(applicationName.ToLower(), null);
            }
            return storageToUse.ContainsKey(key) ? storageToUse[key] : defaultSet;
        }

        public static DataSetProvider GetInstance() {
            return _instance ??
                   (_instance =
                       SimpleInjectorGenericFactory.Instance.GetObject<DataSetProvider>(typeof(DataSetProvider)));
        }

        class DataSetKey {
            readonly string _application;
            readonly string _client;

            public DataSetKey(string application, string client) {
                _application = application;
                _client = client;
            }

            private bool Equals(DataSetKey other) {
                var applicationEquals = string.Equals(_application, other._application,StringComparison.CurrentCultureIgnoreCase);
                var clientEquals = _client == null || string.Equals(_client, other._client,StringComparison.CurrentCultureIgnoreCase);
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
                    return ((_application != null ? _application.ToLower().GetHashCode() : 0) * 397) ^ (_client != null ? _client.ToLower().GetHashCode() : 0);
                }
            }

            public override string ToString() {
                return string.Format("Application: {0}, Client: {1}", _application, _client);
            }
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            var dataSets = SimpleInjectorGenericFactory.Instance.GetObjectsOfType<IDataSet>(typeof(IDataSet));

            foreach (var dataSet in dataSets) {
                //                if (dataSetType.IsInterface || dataSetType.IsAbstract) {
                //                    continue;
                //                }
                //                var dataSet = (IDataSet)Activator.CreateInstance(dataSetType);
                var applicationName = dataSet.ApplicationName();
                if (applicationName == null) {
                    //null stands for framework instances... we dont need to handle these
                    continue;
                }

                var isSWDBApplication = applicationName.StartsWith("_");
                var isSWDDBDataSet = dataSet is SWDBApplicationDataset;
                if (isSWDDBDataSet && !isSWDBApplication) {
                    throw DataSetConfigurationException.SWDBApplicationRequired(dataSet.GetType());
                }
                var storageToUse = isSWDBApplication ? _swdbDataSets : _maximoDataSets;

                var clientFilter = dataSet.ClientFilter();
                if (clientFilter != null) {
                    var strings = clientFilter.Split(',');
                    foreach (var client in strings) {
                        storageToUse.Add(new DataSetKey(applicationName, client), dataSet);
                    }
                } else {
                    storageToUse.Add(new DataSetKey(applicationName, null), dataSet);
                }
            }
        }
    }
}
