using System;
using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared2.Data {
    public class DataMapDefinition : AttributeHolder {
        public string Application { get; set; }
        public long? Approwstamp { get; set; }

        public string Id { get; set; }


        public DataMapDefinition() { }

        public DataMapDefinition(string application, IDictionary<string, object> fields)
            : base(fields) {
            if (application == null) throw new ArgumentNullException("application");
            if (fields == null) throw new ArgumentNullException("fields");

            Application = application;
            //            HandleRowStamps(fields);
        }

        //        private void HandleRowStamps(IDictionary<string, object> fields) {
        //            //TODO: handle associations correctly on entitymetadataslicer, rowstamps should not be here!
        //            var rowstampFields = new Dictionary<string, object>();
        //            foreach (var pair in fields) {
        //                if (pair.Key == RowStampUtil.RowstampColumnName || pair.Key.Contains("." + RowStampUtil.RowstampColumnName)) {
        //                    rowstampFields.Add(pair.Key, RowStampUtil.Convert(pair.Value));
        //                }
        //            }
        //
        //            foreach (var o in rowstampFields) {
        //                fields[o.Key] = o.Value;
        //            }
        //
        //
        //            object rowstampObject;
        //            if (fields.TryGetValue(RowStampUtil.RowstampColumnName, out rowstampObject)) {
        //                _approwstamp = (long)rowstampObject;
        //            }
        //        }

        public IDictionary<string, object> Fields {
            get { return Attributes; }
        }


        public string Value(string name) {
            if (name == null) throw new ArgumentNullException("name");

            return Attributes[name].ToString();
        }

        public string Value(ApplicationFieldDefinition field) {
            if (field == null) throw new ArgumentNullException("field");

            return Attributes[field.Attribute].ToString();
        }

        public T Value<T>(string name) {
            if (name == null) throw new ArgumentNullException("name");

            return (T)Convert.ChangeType(Attributes[name], typeof(T), null);
        }

        public void Value(string name, string value) {
            if (name == null) throw new ArgumentNullException("name");

            Attributes[name] = value;
        }

        public void Value(ApplicationFieldDefinition field, string value) {
            if (field == null) throw new ArgumentNullException("field");

            Attributes[field.Attribute] = value;
        }



        //        
        //        public static DataMap Populate(ApplicationSchemaDefinition ApplicationSchemaDefinition,
        //                                       IEnumerable<KeyValuePair<string, object>> row) {
        //            IDictionary<string, object> attributes = new Dictionary<string, object>();
        //            foreach (var pair in row) {
        //                object value;
        //                if (pair.Key == "rowstamp" || pair.Key.Contains(".rowstamp")) {
        //                    value = RowStampUtil.Convert(pair.Value);
        //                } else {
        //                    value = Convert.ToString(pair.Value);
        //                }
        //                attributes[pair.Key] = value;
        //            }
        //            return new DataMap(ApplicationSchemaDefinition.Name, attributes);
        //        }


        public override string HolderName() {
            return Application;
        }
    }
}
