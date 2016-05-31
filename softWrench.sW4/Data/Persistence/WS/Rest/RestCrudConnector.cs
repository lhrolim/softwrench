using System;
using System.Linq;
using System.Security.Policy;
using System.Xml.Linq;
using cts.commons.portable.Util;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Parsing;
using WcfSamples.DynamicProxy;

namespace softWrench.sW4.Data.Persistence.WS.Rest {
    public class RestCrudConnector : BaseMaximoCrudConnector {

        public override DynamicObject CreateProxy(EntityMetadata metadata) {
            return null;
        }

        public override MaximoOperationExecutionContext CreateExecutionContext(DynamicObject proxy, IOperationData operationData) {
            return new RestExecutionContext((CrudOperationData)operationData);
        }

        public override void DoCreate(MaximoOperationExecutionContext maximoTemplateData) {

            var resultData = maximoTemplateData.InvokeProxy();

            maximoTemplateData.ResultObject = ParseResult(maximoTemplateData.Metadata, (string)resultData);

        }

        private static XElement GetResultElement(XElement xml, EntityMetadata entityMetadata) {
            var rootSetElem = xml.Descendants().FirstOrDefault(f => f.Name.LocalName.EqualsIc(entityMetadata.ConnectorParameters.GetWSEntityKey() + "Set"));
            if (rootSetElem == null) {
                return xml;
            }
            return rootSetElem.Elements().FirstOrDefault();
        }

        internal static TargetResult ParseResult(EntityMetadata entityMetadata, string resultData) {
            var idProperty = entityMetadata.Schema.IdAttribute.Name;
            var siteIdAttribute = entityMetadata.Schema.SiteIdAttribute;
            var userIdProperty = entityMetadata.Schema.UserIdAttribute.Name;
            var xml = XElement.Parse(resultData);

            var resultElement = GetResultElement(xml,entityMetadata);
            if (resultElement == null) {
                return null;
            }

            var id = resultElement.ElementValue(idProperty);
            var userId = resultElement.ElementValue(userIdProperty);

            string siteId = null;
            if (siteIdAttribute != null) {
                //not all entities will have a siteid...
                siteId = resultElement.ElementValue(siteIdAttribute.Name);
            }

            if (!idProperty.Equals(userIdProperty) && userId == null) {
                Log.WarnFormat("User Identifier {0} not received after creating object in Maximo.", idProperty);
                return new TargetResult(null, null, resultData, null, siteId);
            }
            if (id == null && userId == null) {
                Log.WarnFormat("Identifier {0} not received after creating object in Maximo.", idProperty);
                return new TargetResult(null, null, resultData, null, siteId); ;
            }
            if (id == null) {
                Log.WarnFormat("Identifier {0} not received after creating object in Maximo.", idProperty);
                return new TargetResult(null, userId.ToString(), resultData, null, siteId);
            }
            return new TargetResult(id.ToString(), userId.ToString(), resultData, null, siteId);
        }
    }
}
