using System;
using System.Collections.Generic;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;

namespace softwrench.sw4.api.classes.application {
    public abstract class ApplicationFiltereableProvider<T> : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> where T : IApplicationFiltereable {

        private readonly IDictionary<ApplicationFiltereableKey, T> _defaultStorage = new Dictionary<ApplicationFiltereableKey, T>();


        public virtual T LookupItem(String applicationName, string schemaId, string clientName) {
            var storage = LocateStorageByName(applicationName);
            //first we try a perfect match: app + client + schema
            var key = LookUp(applicationName, schemaId, clientName, storage);
            //if not found return the default
            return storage.ContainsKey(key) ? storage[key] : LocateDefaultItem(applicationName, schemaId, clientName);
        }



        protected abstract T LocateDefaultItem(string applicationName, string schemaId, string clientName);


        protected ApplicationFiltereableKey LookUp(string applicationName, string schemaId, string clientName, IDictionary<ApplicationFiltereableKey, T> storageToUse = null) {
            var key = new ApplicationFiltereableKey(applicationName, clientName, schemaId);
            if (storageToUse == null) {
                storageToUse = _defaultStorage;
            }

            if (storageToUse.ContainsKey(key)) {
                return key;
            }
            //try otb as fallback
            key = new ApplicationFiltereableKey(applicationName, "otb", schemaId);
            if (storageToUse.ContainsKey(key)) {
                return key;
            }

            //second just app + schema
            key = new ApplicationFiltereableKey(applicationName, null, schemaId);
            if (storageToUse.ContainsKey(key)) {
                return key;
            }
            //app + client
            key = new ApplicationFiltereableKey(applicationName, clientName, null);
            if (storageToUse.ContainsKey(key)) {
                return key;
            }
            key = new ApplicationFiltereableKey(applicationName, "otb", null);
            if (storageToUse.ContainsKey(key)) {
                return key;
            }
            //last just app
            return new ApplicationFiltereableKey(applicationName, null, null);
        }

        public virtual void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            Clear();
            var dataSets = SimpleInjectorGenericFactory.Instance.GetObjectsOfType<T>(typeof(T));

            foreach (var dataSet in dataSets) {
                var applicationNames = dataSet.ApplicationName();

                if (applicationNames == null) {
                    //null stands for framework instances... we dont need to handle these
                    continue;
                }
                var schemaId = dataSet.SchemaId();

                var storageToUse = LocateStorage(dataSet);

                var clientFilter = dataSet.ClientFilter();
                foreach (string applicationName in applicationNames.Split(',')) {
                    if (clientFilter != null) {
                        var strings = clientFilter.Split(',');
                        foreach (var client in strings) {
                            storageToUse.Add(new ApplicationFiltereableKey(applicationName, client, schemaId), dataSet);
                        }
                    } else {
                        storageToUse.Add(new ApplicationFiltereableKey(applicationName, null, schemaId), dataSet);
                    }
                }
            }
        }

        public virtual void Clear() {
            _defaultStorage.Clear();
        }

        protected virtual IDictionary<ApplicationFiltereableKey, T> LocateStorageByName(string applicationName) {
            return _defaultStorage;
        }

        protected virtual IDictionary<ApplicationFiltereableKey, T> LocateStorage(T item) {
            return _defaultStorage;
        }

    }
}
