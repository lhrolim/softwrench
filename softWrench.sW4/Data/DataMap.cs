using JetBrains.Annotations;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Metadata.Applications;
using System;
using System.Collections.Generic;
using softWrench.sW4.AUTH;

namespace softWrench.sW4.Data {
    public class DataMap : DataMapDefinition {



        public DataMap([NotNull] string application, string idFieldName,[NotNull] IDictionary<string, object> fields, bool rowstampsHandled = false)
            : base(application, fields) {
            if (!rowstampsHandled) {
                HandleRowStamps(fields);
            }
            object rowstampObject;
            if (fields.TryGetValue(RowStampUtil.RowstampColumnName, out rowstampObject)) {
                Approwstamp = (long)rowstampObject;
            }
            if (fields.ContainsKey(idFieldName) && fields[idFieldName]!=null) {
                fields.Add("hmachash", AuthUtils.HmacShaEncode(fields[idFieldName].ToString()));
            }
            if (fields.ContainsKey("hlagchangeticketid") && fields["hlagchangeticketid"] != null) {
                fields.Add("srhmachash", AuthUtils.HmacShaEncode(fields["hlagchangeticketid"].ToString()));
            }
            

        }

        private void HandleRowStamps(IDictionary<string, object> fields) {
            //TODO: handle associations correctly on entitymetadataslicer, rowstamps should not be here!
            var rowstampFields = new Dictionary<string, object>();
            foreach (var pair in fields) {
                if (pair.Key == RowStampUtil.RowstampColumnName || pair.Key.Contains("." + RowStampUtil.RowstampColumnName)) {
                    rowstampFields.Add(pair.Key, RowStampUtil.Convert(pair.Value));
                }
            }
            foreach (var o in rowstampFields) {
                fields[o.Key] = o.Value;
            }
        }

        [NotNull]
        public static DataMap Populate(ApplicationMetadata applicationMetadata,
                                       IEnumerable<KeyValuePair<string, object>> row) {
            IDictionary<string, object> attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in row) {
                object value;
                if (pair.Key == RowStampUtil.RowstampColumnName || pair.Key.Contains("." + RowStampUtil.RowstampColumnName)) {
                    value = RowStampUtil.Convert(pair.Value);
                } else {
                    value = Convert.ToString(pair.Value);
                }
                attributes[pair.Key] = value;
            }
            //true: avoid double rows interation for rowstamp handling
            return new DataMap(applicationMetadata.Name,applicationMetadata.IdFieldName, attributes, true);
        }


     
    }
}
