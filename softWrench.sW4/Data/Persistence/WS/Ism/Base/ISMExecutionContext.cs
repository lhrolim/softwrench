using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using log4net;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;
using WcfSamples.DynamicProxy;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Base {
    class IsmExecutionContext : MaximoOperationExecutionContext {

        private static readonly ILog Log = LogManager.GetLogger(typeof(IsmExecutionContext));

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

        public override object InvokeProxy() {
            var arg0 = SerializeIntegrationObject();
            try {

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var soapEnvelopeXml = new XmlDocument();

                var isChange = Metadata.Name.Equals("wochange", StringComparison.InvariantCultureIgnoreCase);
                var path = isChange ? MetadataProvider.GlobalProperty("globaservletpath_chg") : MetadataProvider.GlobalProperty("globaservletpath_inc");
            
                Log.DebugFormat("Calling ISM WS on {0}. Content: {1}", path, arg0);
                soapEnvelopeXml.LoadXml(@arg0);
                var webRequest = CreateWebRequest(path);
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
            } catch (Exception e) {
                Log.Error("Error invoking ISM proxy", e);
                var rootException = ExceptionUtil.DigRootException(e);
                throw rootException;
            }
        }

        private HttpWebRequest CreateWebRequest(string path)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(path);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            var authInfo = ApplicationConfiguration.IsmCredentialsUser + ":" + ApplicationConfiguration.IsmCredentialsPassword;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            webRequest.Headers["Authorization"] = "Basic " + authInfo;
            return webRequest;
        }

        private string SerializeIntegrationObject() {
            var rootElement = IntegrationObject;
            var serializer = new XmlSerializer(rootElement.GetType(), @"http://b2b.ibm.com/schema/B2B_CDM_Incident/R2_2");
            var sWriter = new StringWriter();
            serializer.Serialize(sWriter, rootElement);
            return sWriter.ToString().Substring(sWriter.ToString().IndexOf('\n') + 1, sWriter.ToString().Length - sWriter.ToString().IndexOf('\n') - 1);
        }

        protected override string MethodName() {
            return "ProcessDoc";
        }

        internal override object FindById(object id) {
            throw new NotImplementedException();
        }

    }
}
