﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata {


    public class MetadataBuilderUtil {
        public const string DB2Query = "SELECT colname as name,typename as type,nulls FROM SYSCAT.COLUMNS where TABNAME  = ?";
        public const string MSSQLQuery = "select COLUMN_NAME as name ,DATA_TYPE as type,IS_NULLABLE from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME= ?";
        private MaximoHibernateDAO _dao = new MaximoHibernateDAO();

        public string GenerateEntityMetadata(string tableName) {
            const ApplicationConfiguration.DBType dbType = ApplicationConfiguration.DBType.Maximo;


            if (ApplicationConfiguration.IsDB2(dbType)) {
                return HandleGeneric(DB2Query, tableName, Db2Delegate);
            }
            if (ApplicationConfiguration.IsMSSQL(dbType)) {
                return HandleGeneric(MSSQLQuery, tableName, SQLServerDelegate);
            }
            throw new NotSupportedException();
        }

        private static void Db2Delegate(XmlElement attribute, IEnumerable<KeyValuePair<string, object>> row) {
            foreach (var keyValuePair in row) {
                var key = keyValuePair.Key.ToLower();
                var value = keyValuePair.Value.ToString().ToLower();
                if (key.ToLower() == "nulls") {
                    attribute.SetAttribute("required", (value == "n").ToString().ToLower());
                }
                else {
                    attribute.SetAttribute(key, value);
                }
            }
        }

        private static void SQLServerDelegate(XmlElement attribute, IEnumerable<KeyValuePair<string, object>> row) {
            foreach (var keyValuePair in row) {
                var key = keyValuePair.Key.ToLower();
                var value = keyValuePair.Value.ToString().ToLower();
                if (key.ToUpper() == "IS_NULLABLE") {
                    attribute.SetAttribute("required", (value.ToLower() == "no").ToString().ToLower());
                }
                else {
                    attribute.SetAttribute(key, value);
                }
            }
        }

        private string HandleGeneric(string query, string tableName, Action<XmlElement, IEnumerable<KeyValuePair<string, object>>> rowDelegate) {
            var result = _dao.FindByNativeQuery(query, tableName.ToUpper());
            if (result == null) {
                return null;
            }
            var rows = result.Cast<IEnumerable<KeyValuePair<string, object>>>();
            rows = rows as IList<IEnumerable<KeyValuePair<string, object>>> ?? rows.ToList();
            var enumerable = rows as IEnumerable<KeyValuePair<string, object>>[] ?? rows.ToArray();
            if (!enumerable.Any()) {
                return null;
            }
            var doc = new XmlDocument();
            var el = (XmlElement)doc.AppendChild(doc.CreateElement("entity"));
            el.SetAttribute("name", tableName);
            var attributes = doc.CreateElement("attributes");
            el.AppendChild(attributes);


            foreach (var row in enumerable) {
                var attribute = doc.CreateElement("attribute");
                attributes.AppendChild(attribute);
                rowDelegate(attribute, row);

            }
            var connectorParams = doc.CreateElement("connectorParameters");
            var connectorParam = doc.CreateElement("connectorParameter");
            connectorParam.SetAttribute("key", "dbtable");
            connectorParam.SetAttribute("value", tableName);
            el.AppendChild(connectorParams);
            connectorParams.AppendChild(connectorParam);
            return doc.OuterXml;
        }
    }
}
