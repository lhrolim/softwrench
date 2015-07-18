using System;
using System.Collections.Generic;
using softWrench.sW4.Metadata.Applications.DataSet.baseclasses;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.DataSet {

    public class DataSetProvider : ApplicationFiltereableProvider<IDataSet> {

        private static DataSetProvider _instance;

        private readonly BaseApplicationDataSet _defaultMaximoDataSet;


        private readonly IDictionary<ApplicationFiltereableKey, IDataSet> _maximoDataSets = new Dictionary<ApplicationFiltereableKey, IDataSet>();


        public DataSetProvider(BaseApplicationDataSet defaultMaximoDataSet) {
            _defaultMaximoDataSet = defaultMaximoDataSet;
        }

        public IDataSet LookupDataSet(String applicationName, string schemaId = null) {

            var clientName = ApplicationConfiguration.ClientName;
            return base.LookupItem(applicationName, schemaId, clientName);
        }


        public static DataSetProvider GetInstance() {
            return _instance ??
                   (_instance =
                       SimpleInjectorGenericFactory.Instance.GetObject<DataSetProvider>(typeof(DataSetProvider)));
        }


        protected override IDataSet LocateDefaultItem(string applicationName, string schemaId, string clientName) {
            return _defaultMaximoDataSet;
        }

        protected override IDictionary<ApplicationFiltereableKey, IDataSet> LocateStorage(IDataSet item) {

            return _maximoDataSets;

        }

        protected override IDictionary<ApplicationFiltereableKey, IDataSet> LocateStorageByName(string applicationName) {
            return _maximoDataSets;
        }

        public BaseApplicationDataSet LookupAsBaseDataSet(string applicationName) {
            return (BaseApplicationDataSet)LookupDataSet(applicationName);
        }
    }
}
