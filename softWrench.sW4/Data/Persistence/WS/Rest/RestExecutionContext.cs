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
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Internal.Constants;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Util;
using CompressionUtil = cts.commons.Util.CompressionUtil;

namespace softWrench.sW4.Data.Persistence.WS.Rest {
    public class RestExecutionContext : MaximoOperationExecutionContext {


        private readonly string _baseRestURL;
        private readonly bool _isUpdate;



        public RestExecutionContext(CrudOperationData operationData) : base(operationData) {
            _baseRestURL = GenerateRestUrl(operationData.EntityMetadata, operationData.Id);
            //this would simply hold the form parameters, instead of an ordinary SOAP xsd schema
            IntegrationObject = new RestIntegrationObjectWrapper();
            _isUpdate = operationData.Id != null;
        }

        private string GenerateRestUrl(EntityMetadata entityMetadata, string entityId) {
            var baseRestURL = MetadataProvider.GlobalProperty(MaximoRestUtils.BaseRestURLProp);
            var entityKey = entityMetadata.ConnectorParameters.GetWSEntityKey(ConnectorParameters.UpdateInterfaceParam, WsProvider.REST);
            if (baseRestURL.EndsWith("/mbo/")) {
                if (entityKey.StartsWith("SW")) {
                    entityKey = entityKey.Substring(2);
                }
            }

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

        public override object FindById(object id) {
            throw new NotImplementedException();
        }

        protected override TargetResult DoProxyInvocation() {
            //            if (ApplicationConfiguration.IgnoreWsCertErrors) {
            //                ServicePointManager.ServerCertificateValidationCallback = delegate {
            //                    return true;
            //                };
            //                ServicePointManager.Expect100Continue = true;
            //                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
            //                    | SecurityProtocolType.Tls11
            //                    | SecurityProtocolType.Tls12
            //                    | SecurityProtocolType.Ssl3;
            //            }
            var headers = MaximoRestUtils.GetMaximoHeaders();

            var payLoad = GeneratePayLoad();

            if (Log.IsDebugEnabled) {
                Log.DebugFormat("invoking web service at {0} with headers {1} and payload {2}", _baseRestURL, headers, payLoad);
            }

            var resultData = RestUtil.CallRestApiSync(_baseRestURL, MethodName(), headers, payLoad);
            return CreateResultData(resultData);
        }



        protected override Exception HandleProxyInvocationError(Exception e) {
            if (e is WebException) {
                return MaximoException.ParseWebExceptionResponse((WebException)e);
            }
            return base.HandleProxyInvocationError(e);
        }



        private string GeneratePayLoad() {
            var obj = (RestIntegrationObjectWrapper)IntegrationObject;
            var dict = obj.Entries;

            var sb = new StringBuilder();
            sb.Append("_action=AddChange");
            foreach (var entry in dict) {
                if (!(entry.Value is RestComposedData) && !obj.HasNonInlineComposition) {
                    //appending non-composition fields only if we do not have a composition present on the datamap
                    if (entry.Key.Contains(".")) {
                        Log.WarnFormat("ignoring entry {0} for url call {1}", entry.Key, _baseRestURL);
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
                } else if (entry.Key.Equals("PLUSPCUSTOMER", StringComparison.InvariantCultureIgnoreCase)) {
                    //making sure we're always passing the pluspcustomer, regardless
                    sb.Append("&").Append(entry.Key).Append("=");
                    sb.Append(WebUtility.UrlEncode(entry.Value.ToString()));
                }

                if (entry.Value is RestComposedData) {
                    var composed = (RestComposedData)entry.Value;
                    sb.Append("&").Append(composed.SerializeParameters());
                }
            }


            return sb.ToString();
        }


    }
}
