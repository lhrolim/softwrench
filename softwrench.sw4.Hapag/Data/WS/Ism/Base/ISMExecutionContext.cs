using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using cts.commons.Util;
using log4net;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;
using WcfSamples.DynamicProxy;

namespace softwrench.sw4.Hapag.Data.WS.Ism.Base {
    class IsmExecutionContext : MaximoOperationExecutionContext {

        

        private new static readonly ILog Log = LogManager.GetLogger(WsInputLog);

        public IsmExecutionContext(DynamicObject proxy, IOperationData operationData)
            : base(operationData) {
            Proxy = proxy;
            IntegrationObject = InstantiateIntegrationObject(operationData.EntityMetadata);
        }

        private object InstantiateIntegrationObject(EntityMetadata entityMetadata) {
            if (entityMetadata.Name.Equals("SR", StringComparison.InvariantCultureIgnoreCase) ||
                entityMetadata.Name.Equals("INCIDENT", StringComparison.CurrentCultureIgnoreCase) ||
                entityMetadata.Name.Equals("PROBLEM", StringComparison.CurrentCultureIgnoreCase) ||
                entityMetadata.Name.Equals("IMAC", StringComparison.CurrentCultureIgnoreCase) ||
                entityMetadata.Name.Equals("NEWCHANGE", StringComparison.CurrentCultureIgnoreCase)) {
                return new ServiceIncident() {
                    Transaction = new Transaction()
                };
            } if (entityMetadata.Name.Equals("WOCHANGE", StringComparison.CurrentCultureIgnoreCase)) {
                return new ChangeRequest() {
                    Transaction = new Transaction()
                };
            }
            throw new NotSupportedException(String.Format("entity {0} is not supported", entityMetadata.Name));
        }

        protected override object DoProxyInvocation() {
            var arg0 = SerializeIntegrationObject();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var soapEnvelopeXml = new XmlDocument();

            var isChange = Metadata.Name.Equals("wochange", StringComparison.InvariantCultureIgnoreCase);
            var path = isChange ? MetadataProvider.GlobalProperty("globaservletpath_chg") : MetadataProvider.GlobalProperty("globaservletpath_inc");

            Log.InfoFormat("PERFORMANCE - ISM WS request started at {0}.", DateTime.Now);
            Log.DebugFormat("Calling ISM WS on {0}. Content: {1}", path, arg0);
            soapEnvelopeXml.LoadXml(@arg0);
            var webRequest = CreateWebRequest(path);

            webRequest.Timeout = ApplicationConfiguration.MaximoRequestTimeout;

            using (Stream stream = webRequest.GetRequestStream()) {
                soapEnvelopeXml.Save(stream);
            }
            // begin async call to web request.
            var asyncResult = webRequest.BeginGetResponse(null, null);

            // suspend this thread until call is complete. You might want to
            // do something usefull here like update your UI.
            asyncResult.AsyncWaitHandle.WaitOne();

            // get the response from the completed web request.
            var result = "";
            using (var webResponse = webRequest.EndGetResponse(asyncResult)) {
                using (var rd = new StreamReader(webResponse.GetResponseStream())) {
                    result = rd.ReadToEnd();
                }
            }

            return result;
        }

        protected override Exception HandleProxyInvocationError(Exception e) {
            Log.Error("Error invoking ISM proxy", e);
            return base.HandleProxyInvocationError(e);
        }

        protected override void OnProxyInvocationComplete(Stopwatch beforeInvocation) {
            var msDelta = LoggingUtil.MsDelta(beforeInvocation);
            Log.InfoFormat("PERFORMANCE - ISM WS request took {0} ms to be executed.", msDelta);
        }

        private HttpWebRequest CreateWebRequest(string path) {
            var webRequest = (HttpWebRequest)WebRequest.Create(path);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            var authInfo = ApplicationConfiguration.IsmCredentialsUser + ":" + ApplicationConfiguration.IsmCredentialsPassword;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            webRequest.Headers["Authorization"] = "Basic " + authInfo;
            return webRequest;
        }
       

        protected override string MethodName() {
            return "ProcessDoc";
        }

        public override object FindById(object id) {
            throw new NotImplementedException();
        }

    }
}
