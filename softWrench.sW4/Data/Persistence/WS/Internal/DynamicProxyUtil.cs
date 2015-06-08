using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using cts.commons.portable.Util;
using log4net;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector.Events;
using WcfSamples.DynamicProxy;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;

namespace softWrench.sW4.Data.Persistence.WS.Internal {
    internal class DynamicProxyUtil : ISWEventListener<ClearCacheEvent> {

        private const string MissingKeyMsg = "Please provide integration_interface key for entity {0}";

        private const string QueryInterfaceParam = "integration_query_interface";
        private static readonly Dictionary<String, IDynamicProxyFactory> DynamicProxyCache = new Dictionary<string, IDynamicProxyFactory>();
        private readonly static ILog Log = LogManager.GetLogger(typeof(DynamicProxyUtil));

        //TODO: adjust caching
        public static DynamicObject LookupProxy(EntityMetadata metaData, Boolean queryProxy = false) {
            var wsdlUri = GetWsdlUri(metaData, queryProxy);
            try {
                
                var factory = LookupFactory(wsdlUri);
                return factory.CreateMainProxy();
            } catch (Exception e) {
                var httpException =new HttpRequestException("wsdl cannot be downloaded at {0}".Fmt(wsdlUri),e);
                Log.Error("Error LookupProxy", httpException);
                throw httpException;
            }
        }

        public static DynamicObject LookupProxy(String integrationInterface, Boolean applyPrefix = true) {
            string wsdlUri = GetWsdlFromKey(integrationInterface, applyPrefix);
            var factory = LookupFactory(wsdlUri);
            return factory.CreateMainProxy();
        }

        public static void ClearCache() {
            DynamicProxyCache.Clear();
        }


        private static IDynamicProxyFactory LookupFactory(String wsdlUri) {
            if (DynamicProxyCache.ContainsKey(wsdlUri)) {
                return DynamicProxyCache[wsdlUri];
            }
            var factory = GetFactory(wsdlUri);
            DynamicProxyCache[wsdlUri] = factory;
            return factory;
        }

        private static IDynamicProxyFactory GetFactory(String wsdlUri) {
            Log.InfoFormat("Looking for Dynamic Proxy factory at wsdl {0}", wsdlUri);

            if (ApplicationConfiguration.IgnoreWsCertErrors) {
                ServicePointManager.ServerCertificateValidationCallback = SWIgnoreErrorsCertHandler;
            }

            if (ApplicationConfiguration.IsMea()) {
                // TODO: support authenticated mea
                return new DynamicProxyFactory(wsdlUri);
            }

            var credentialsUser = ApplicationConfiguration.MifCredentialsUser;
            var credentialsPassword = ApplicationConfiguration.MifCredentialsPassword;
            if (string.IsNullOrEmpty(credentialsUser) || string.IsNullOrEmpty(credentialsPassword)) {
                return new AsmxDynamicProxyFactory(wsdlUri);
            } else {
                return new AsmxDynamicProxyFactory(wsdlUri, credentialsUser, credentialsPassword);
            }
        }
        static bool SWIgnoreErrorsCertHandler(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors error) {
            // Ignore SSL errors
            return true;
        }

        private static string GetWsdlUri(EntityMetadata metaData, bool queryProxy) {
            String entityKey = null;
            var connectorParams = metaData.ConnectorParameters.Parameters;
            var keyToUse = queryProxy && ApplicationConfiguration.IsMea() ? QueryInterfaceParam : ConnectorParameters.UpdateInterfaceParam;
            //first we try mea_integration_interface / mif_integration_interface
            entityKey = metaData.ConnectorParameters.GetWSEntityKey(keyToUse);
            if (!string.IsNullOrEmpty(entityKey)) {
                return GetWsdlFromKey(entityKey);
            }

            //fallback if we are talking about queryproxy...
            if (queryProxy) {
                if (!connectorParams.TryGetValue(ApplicationConfiguration.WsProvider + "_" + ConnectorParameters.UpdateInterfaceParam,
                    out entityKey)
                    && !connectorParams.TryGetValue(ConnectorParameters.UpdateInterfaceParam, out entityKey)) {
                    throw new InvalidOperationException(String.Format(MissingKeyMsg, metaData.Name));
                }
                //by convention, query interface name would be IN_NAME + "_QUERY" (SWWO ==> SWWO_QUERY)
                entityKey = entityKey + "_QUERY";
            } else {
                throw new InvalidOperationException(String.Format(MissingKeyMsg, metaData.Name));
            }
            return GetWsdlFromKey(entityKey);
        }

        private static string GetWsdlFromKey(string entityKey, bool applyPrefix = true) {

            if (!entityKey.EndsWith("wsdl")) {
                if (SwConstants.WsProvider == "mea") {
                    entityKey = entityKey + "?wsdl";
                } else {
                    entityKey = entityKey + ".wsdl";
                }
            }
            if (!applyPrefix) {
                return entityKey;
            }
            if (Log.IsDebugEnabled) {
                Log.DebugFormat("building wsdl url: basewsURL property={0}, entitykey={1}", ApplicationConfiguration.WsUrl, entityKey);
            }
            return ApplicationConfiguration.WsUrl + entityKey;
        }


        public static Type[] TypesFromParameters(IList<object> parameterList) {
            var arr = new Type[parameterList.Count()];
            for (int i = 0; i < parameterList.Count(); i++) {
                var value = parameterList[i];
                if (value != null) {
                    arr.SetValue(value.GetType(), i);
                }
            }
            return arr;
        }

        public void HandleEvent(ClearCacheEvent eventToDispatch) {
            ClearCache();
        }
    }
}
