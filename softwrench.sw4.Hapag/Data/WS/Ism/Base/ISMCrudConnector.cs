using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Xml;
using log4net;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using WcfSamples.DynamicProxy;

namespace softwrench.sw4.Hapag.Data.WS.Ism.Base {
    internal sealed class IsmCrudConnector : BaseMaximoCrudConnector {

        private new static readonly ILog Log = LogManager.GetLogger(typeof(IsmCrudConnector));

        public override DynamicObject CreateProxy(EntityMetadata metadata) {
            //for ISM we have only a single wsdl for all the web-services
            //so we don´t need to specify one for each of the entities, like the other maximo providers
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
            delegate { return true; }
            );
            var wsdlPath = MetadataProvider.GlobalProperty(ISMConstants.GlobalWsdlProperty);
//            return DynamicProxyUtil.LookupProxy(wsdlPath, false);
            return null;
        }



        public override void DoCreate(MaximoOperationExecutionContext maximoTemplateData) {
            var resultData = maximoTemplateData.InvokeProxy();
            var data = resultData.ToString();
            Log.DebugFormat("Receiving response from ISM creation {0}", data);
            var resultDataValues = ParseResultData(data);
            var idProperty = resultDataValues["requesterid"];
            var transactionComment = resultDataValues["comment"];
            if (transactionComment != "Success - No Errors Encountered") {
                throw new Exception("ISM Web Service Did Not Return a Successful Response: " + transactionComment);
            }
            maximoTemplateData.ResultObject = new TargetResult(idProperty, idProperty, null);
        }

        public override void DoUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var resultData = maximoTemplateData.InvokeProxy();
            var data = resultData.ToString();
            Log.DebugFormat("Receiving response from ISM update {0}", data);
            var resultDataValues = ParseResultData(data);
            var idProperty = resultDataValues["requesterid"];
            var transactionComment = resultDataValues["comment"];
            if (transactionComment != "Success - No Errors Encountered") {
                throw new Exception("ISM Web Service Did Not Return a Successful Response: " + transactionComment);
            }
            maximoTemplateData.ResultObject = new TargetResult(idProperty, idProperty, null);
        }




        private Dictionary<string, string> ParseResultData(string resultData) {/*
            var reader = XmlReader.Create(new StringReader(resultData));
            var nameTable = reader.NameTable;
            var namespaceManager = new System.Xml.XmlNamespaceManager(nameTable);
            namespaceManager.AddNamespace("m1", "http://b2b.ibm.com/schema/IS_B2B_CDM/R2_2");
            namespaceManager.AddNamespace("m2", "http://www.mro.com/mx/integration");
            var doc = XElement.Load(reader);

            var requesterId = from r
                              in doc.Element("RequesterID")
                              select (string)r;*/
            var result = new Dictionary<string, string>();

//            var doc = XDocument.Parse(resultData);
//            var xElement = doc.Descendants().First();
//
//            if (xElement.Name.LocalName == "ServiceIncident") {
//                var xElements = xElement.DescendantNodes();
//                foreach (var node in xElements) {
//                    if (node.NodeType != XmlNodeType.Element) {
//                        continue;
//                    }
//                    var element = ((XElement)node);
//                    if (element.Name.LocalName == "RequesterID") {
//                        result.Add("requesterid", element.Value);
//                        continue;
//                    }
//                    if (element.Name.LocalName != "Transaction") {
//                        continue;
//                    }
//                    foreach (var transNode in element.DescendantNodes()) {
//                        if (transNode.NodeType == XmlNodeType.Element) {
//                            var transElement = ((XElement)transNode);
//                            if (transElement.Name.LocalName == "Comment") {
//                                result.Add("comment", transElement.Value);
//                            }
//                        }
//                    }
//                }
//            }
//            string reqId;
//            string comment;
//            if (result.TryGetValue("requesterid", out reqId) == false) {
//                throw new Exception("No Requester ID Returned by ISM");
//            }
//            if (result.TryGetValue("comment", out comment) == false) {
//                throw new Exception("No Transaction Comment Returned by ISM");
//            }

            return result;
        }

        private string GetResultComment(string resultData) {
            var doc = new XmlDocument();
            doc.LoadXml(resultData);
            var transactionList = doc.SelectNodes("//Transaction");
            var transactionComment = transactionList[0].SelectNodes("//Comment");
            return transactionComment[0].InnerText;

        }



        public override MaximoOperationExecutionContext CreateExecutionContext(DynamicObject proxy, IOperationData operationData) {
            return new IsmExecutionContext(proxy, operationData);
        }
    }
}
