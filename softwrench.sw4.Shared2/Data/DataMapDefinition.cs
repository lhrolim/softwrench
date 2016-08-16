using System;
using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared2.Data {
    public class DataMapDefinition : AttributeHolder {

        private string _application;
        public string Application {
            get {
                return _application;
            }
            set {
                _application = value;
                this["Application"] = value;
            }
        }

        private long? _approwstamp;
        public long? Approwstamp {
            get {
                return _approwstamp;
            }
            set {
                _approwstamp = value;
                this["Approwstamp"] = value;
            }
        }

        private string _id;
        public string Id {
            get {
                return _id;
            }
            set {
                _id = value;
                this["Id"] = value;
            }
        }


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
            get { return this; }
        }


        public string Value(string name) {
            if (name == null) throw new ArgumentNullException("name");

            return this[name].ToString();
        }

        public string Value(ApplicationFieldDefinition field) {
            if (field == null) throw new ArgumentNullException("field");

            return this[field.Attribute].ToString();
        }

        public T Value<T>(string name) {
            if (name == null) throw new ArgumentNullException("name");

            return (T)Convert.ChangeType(this[name], typeof(T), null);
        }

        public void Value(string name, string value) {
            if (name == null) throw new ArgumentNullException("name");

            this[name] = value;
        }

        public void Value(ApplicationFieldDefinition field, string value) {
            if (field == null) throw new ArgumentNullException("field");

            this[field.Attribute] = value;
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
