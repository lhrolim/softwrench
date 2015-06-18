﻿using System;
using System.Collections.Generic;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.api.classes;
using softwrench.sw4.api.classes.application;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.services;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {

    public class DataSetProvider : ApplicationFiltereableProvider<IDataSet> {

        private static DataSetProvider _instance;

        private readonly MaximoApplicationDataSet _defaultMaximoDataSet;
        private readonly MaximoApplicationDataSet _defaultSWDBDataSet;


        private readonly IDictionary<ApplicationFiltereableKey, IDataSet> _maximoDataSets = new Dictionary<ApplicationFiltereableKey, IDataSet>();
        private readonly IDictionary<ApplicationFiltereableKey, IDataSet> _swdbDataSets = new Dictionary<ApplicationFiltereableKey, IDataSet>();

        private readonly IDictionary<ApplicationFiltereableKey, IBatchSubmissionConverter> _batchSubmissionConverter = new Dictionary<ApplicationFiltereableKey, IBatchSubmissionConverter>();

        public DataSetProvider(MaximoApplicationDataSet defaultMaximoDataSet, MaximoApplicationDataSet defaultSWDBDataSet) {
            _defaultMaximoDataSet = defaultMaximoDataSet;
            _defaultSWDBDataSet = defaultSWDBDataSet;
        }

        public IDataSet LookupDataSet(String applicationName, string schemaId) {

            var clientName = ApplicationConfiguration.ClientName;
            return base.LookupItem(applicationName, schemaId, clientName);
        }


        public static DataSetProvider GetInstance() {
            return _instance ??
                   (_instance =
                       SimpleInjectorGenericFactory.Instance.GetObject<DataSetProvider>(typeof(DataSetProvider)));
        }


        protected override IDataSet LocateDefaultItem(string applicationName, string schemaId, string clientName) {
            var isSWDBApplication = applicationName.StartsWith("_");
            return isSWDBApplication ? _defaultSWDBDataSet : _defaultMaximoDataSet;
        }

        protected override IDictionary<ApplicationFiltereableKey, IDataSet> LocateStorage(IDataSet item) {
            var isSWDBApplication = item.ApplicationName().StartsWith("_");
            var isSWDDBDataSet = item is SWDBApplicationDataset;
            if (isSWDDBDataSet && !isSWDBApplication) {
                throw DataSetConfigurationException.SWDBApplicationRequired(item.GetType());
            }
            return isSWDDBDataSet ? _swdbDataSets : _maximoDataSets;
        }

        protected override IDictionary<ApplicationFiltereableKey, IDataSet> LocateStorageByName(string applicationName) {
            var isSWDBApplication = applicationName.StartsWith("_");
            return isSWDBApplication ? _swdbDataSets : _maximoDataSets;
        }
    }
}
