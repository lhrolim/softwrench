using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using log4net;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Data.Persistence.WS.Internal.Constants;
using softWrench.sW4.Data.Persistence.WS.Rest;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Connectors;

namespace softWrench.sW4.Data.Persistence.Relational.EntityRepository {
    public class RestResponseParser : ISingletonComponent {

        private static ILog Log = LogManager.GetLogger(typeof(RestResponseParser));

        public RestResponseParser() {
            Log.Debug("init");
        }

        internal IReadOnlyList<DataMap> ConvertXmlToDatamaps(EntityMetadata entityMetadata, string responseAsText) {
            var result = new List<DataMap>();
            var xml = XElement.Parse(responseAsText);
            return new List<DataMap>(xml.Elements().Select(e => InnerDatamapConversion(entityMetadata, e)));
        }

        internal DataMap ConvertXmlToDatamap(EntityMetadata entityMetadata, string responseAsText, string wsKey = null) {
            var xml = XElement.Parse(responseAsText);
            var resultElement = RestResponseParser.GetResultElement(xml, entityMetadata);
            if (resultElement == null) {
                return null;
            }

            return InnerDatamapConversion(entityMetadata, resultElement);
        }

        internal int TotalCountFromXml(EntityMetadata entityMetadata, string responseAsText, string wsKey = null) {
            var xml = XElement.Parse(responseAsText);
            var resultElement = RestResponseParser.GetResultElement(xml, entityMetadata);
            if (resultElement == null) {
                return 0;
            }
            var totalCountAttribute = resultElement.Attribute("rsTotal");
            return totalCountAttribute == null ? 0 : int.Parse(totalCountAttribute.Value);
        } 

        private static DataMap InnerDatamapConversion(EntityMetadata entityMetadata, XElement resultElement) {
            var result = new Dictionary<string, object>();
            var rowstamp = resultElement.Attribute("rowstamp");
            if (rowstamp != null) {
                result.Add("rowstamp", RowStampUtil.FromStringRepresentation(rowstamp.Value));
            }

            foreach (var element in resultElement.Elements()) {
                var keyName = element.Name.LocalName;
                if (!element.HasElements) {
                    Log.DebugFormat("Adding element {0} with value {1} to datamap", keyName, element.Value);
                    result.Add(keyName, element.Value);
                } else {
                    var relatedMetadata = entityMetadata.RelatedEntityMetadata(keyName);
                    if (relatedMetadata == null) {
                        Log.WarnFormat("ignoring nested element {0} since it´s not declared on the metadata", keyName);
                        //transient relationship 
                        continue;
                    }
                    Log.DebugFormat("Adding nested element {0} to datamap", keyName);
                    if (result.ContainsKey(keyName)) {
                        //convert single elment to list
                        var dm = result[keyName] as DataMap;
                        var list = new List<DataMap>();
                        list.Add(dm);
                        result[keyName] = list;
                        list.Add(InnerDatamapConversion(relatedMetadata, element));
                    } else {
                        result[keyName] = InnerDatamapConversion(relatedMetadata, element);
                    }
                }
            }
            // marking rest origin
            result[MaximoRestUtils.RestMarkerFieldName] = true;
            return new DataMap(entityMetadata.Name, result, null, true);
        }


        public static XElement GetResultElement(XElement xml, EntityMetadata entityMetadata, string wsKey = null) {
            if (wsKey == null) {
                var entityKey = entityMetadata.ConnectorParameters.GetWSEntityKey(ConnectorParameters.UpdateInterfaceParam, WsProvider.REST);
                wsKey = entityKey ?? /* compositions: */ entityMetadata.Name + "Mbo";
            }


            var rootSetElem = xml.Descendants().FirstOrDefault(f => f.Name.LocalName.EqualsIc(wsKey + "Set"));
            if (rootSetElem == null) {
                return xml;
            }
            return rootSetElem.Elements().FirstOrDefault();
        }

    }
}
