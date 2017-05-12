using JetBrains.Annotations;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Metadata.Applications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Data {
    public class DataMap : DataMapDefinition {

        [NotNull]
        public static DataMap BlankInstance(string application) {
            return new DataMap(application, new Dictionary<string, object>(), null, true);
        }

        public DataMap() {

        }

        public DataMap([NotNull] string application, [NotNull] IDictionary<string, object> fields, Type mappingType = null, bool rowstampsHandled = false, string idFieldName = null)
            : base(application, fields) {
            //TODO: apply mapping type properly
            if (!rowstampsHandled) {
                HandleRowStamps(this);
            }
            object rowstampObject;
            if (this.TryGetValue(RowStampUtil.RowstampColumnName, out rowstampObject)) {
                Approwstamp = (long)rowstampObject;
            }
            if (idFieldName != null && ContainsKey(idFieldName)) {
                Id = this[idFieldName].ToString();
            }
        }


        public DataMap([NotNull] string application, [NotNull] IDictionary<string, object> fields, string idFieldName)
            : base(application, fields) {
            HandleRowStamps(this);
            object rowstampObject;
            if (this.TryGetValue(RowStampUtil.RowstampColumnName, out rowstampObject)) {
                Approwstamp = (long)rowstampObject;
            }
            Id = this[idFieldName].ToString();

        }

        private void HandleRowStamps(IDictionary<string, object> fields) {
            //TODO: handle associations correctly on entitymetadataslicer, rowstamps should not be here!
            var rowstampFields = new Dictionary<string, object>();
            foreach (var pair in this) {
                if (pair.Key == RowStampUtil.RowstampColumnName || pair.Key.Contains("." + RowStampUtil.RowstampColumnName)) {
                    rowstampFields.Add(pair.Key, RowStampUtil.Convert(pair.Value));
                }
            }
            foreach (var o in rowstampFields) {
                this[o.Key] = o.Value;
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
                } else if (pair.Value is decimal) {
                    // workaround to remove the trailing zeros after the '.' on decimal values
                    if (pair.Value == null) {
                        value = null;
                    } else {
                        var stringValue = Convert.ToString(pair.Value);
                        stringValue = stringValue.Contains(".") ? stringValue.TrimEnd('0').TrimEnd('.') : stringValue;
                        value = stringValue.Contains(",") ? stringValue.TrimEnd('0').TrimEnd(',') : stringValue;
                    }
                } else {
                    // let the serializer take care of date convertion
                    value = pair.Value == null ? null : (pair.Value is DateTime ? pair.Value : Convert.ToString(pair.Value));
                }
                attributes[pair.Key] = value;
            }
            //true: avoid double rows interation for rowstamp handling
            return new DataMap(applicationMetadata.Name, attributes, null, true) {
                Id = attributes[applicationMetadata.Schema.IdFieldName].ToString()
            };
        }

        public static DataMap GetInstanceFromStringDictionary(string application, IDictionary<string, string> fields) {
            return new DataMap(application, fields.ToDictionary(f => f.Key, f => (object)f.Value));
        }

        public static DataMap GetInstanceFromDictionary(string application, IDictionary<string, object> fields, bool rowstampHandled = false) {
            return new DataMap(application, fields.ToDictionary(f => f.Key, f => f.Value), null, rowstampHandled);
        }

        public string Type => GetType().Name;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extraParameters">on the form param:value,param2:value2</param>
        public DataMap PopulateFromString([CanBeNull]string extraParameters) {
            if (extraParameters == null) {
                return this;
            }

            var parameters = extraParameters.Split(',');
            foreach (var parameter in parameters) {
                var strings = parameter.Split(':');
                SetAttribute(strings[0], strings[1]);
            }
            return this;
        }
    }
}
