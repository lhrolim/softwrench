using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using cts.commons.portable.Util;
using cts.commons.web.Util;
using log4net;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;
using CompressionUtil = cts.commons.Util.CompressionUtil;

namespace softWrench.sW4.Data.Persistence.WS.Rest {
    public class RestExecutionContext : MaximoOperationExecutionContext {


        private readonly string _baseRestURL;
        private readonly bool _isUpdate;

        private static ILog _log = LogManager.GetLogger(typeof(RestExecutionContext));

        public RestExecutionContext(CrudOperationData operationData) : base(operationData) {
            _baseRestURL = GenerateRestUrl(operationData.EntityMetadata, operationData.Id);
            //this would simply hold the form parameters, instead of an ordinary SOAP xsd schema
            IntegrationObject = new RestIntegrationObjectWrapper();
            _isUpdate = operationData.Id != null;
        }

        private string GenerateRestUrl(EntityMetadata entityMetadata, string entityId) {
            var baseRestURL = MetadataProvider.GlobalProperty("basewsRestURL");
            var entityKey = entityMetadata.ConnectorParameters.GetWSEntityKey();
            return !baseRestURL.EndsWith("/") ? baseRestURL + "/" + entityKey : baseRestURL + entityKey + "/" + entityId;
        }

        protected override string MethodName() {
            var obj = (RestIntegrationObjectWrapper)IntegrationObject;
            var dict = obj.Entries;
            //            var method = _isUpdate ? "PUT" : "POST";
            var method = "POST";
            var compositionData = (RestComposedData)dict.Values.FirstOrDefault(v => v is RestComposedData);
            if (compositionData != null) {
                if (compositionData.IsCompositionCreation) {
                    method = "POST";
                }
            }
            return method;
        }

        internal override object FindById(object id) {
            throw new NotImplementedException();
        }

        protected override object DoProxyInvocation() {

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"MAXAUTH", GenerateAuthHeader()},
                {"Content-Type", "application/x-www-form-urlencoded"}
            };

            var callRestApiSync = RestUtil.CallRestApiSync(_baseRestURL, MethodName(), headers, GeneratePayLoad());
            using (var responseStream = callRestApiSync.GetResponseStream()) {
                using (var responseReader = new StreamReader(responseStream)) {
                    // parse xml response
                    var text = responseReader.ReadToEnd();
                    return text;
                }
            }
        }

        private string GeneratePayLoad() {
            var obj = (RestIntegrationObjectWrapper)IntegrationObject;
            var dict = obj.Entries;

            var sb = new StringBuilder();
            sb.Append("_action=AddChange");
            foreach (var entry in dict) {
                if (!(entry.Value is RestComposedData) && !obj.HasNonInlineComposition) {
                    if (entry.Key.Contains(".")) {
                        _log.WarnFormat("ignoring entry {0} for url call {1}", entry.Key, _baseRestURL);
                        continue;
                    }
                    if (entry.Value != null) {
                        sb.Append("&").Append(entry.Key).Append("=");
                        if (entry.Key == "DOCUMENTDATA") {
                            var base64String = (string)entry.Value;
                            sb.Append(base64String.Replace("+", "%2B"));
                        } else {
                            sb.Append(WebUtility.UrlEncode(entry.Value.ToString()));
                        }
                    }
                }

                if (entry.Value is RestComposedData) {
                    var composed = (RestComposedData)entry.Value;
                    sb.Append("&").Append(composed.SerializeParameters());
                }
            }

            return sb.ToString();
        }

        private static string GenerateAuthHeader() {
            var credentialsUser = ApplicationConfiguration.RestCredentialsUser;
            var credentialsPassword = ApplicationConfiguration.RestCredentialsPassword;
            var plainText = "{0}:{1}".Fmt(credentialsUser, credentialsPassword);
            return CompressionUtil.Base64Encode(plainText);
        }
    }
}
