using System;
using System.ComponentModel;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions {
    public class CompositionCollectionProperties {
        [DefaultValue("true")] public string AllowInsertion { get; set; }
        [DefaultValue("false")] public string AllowUpdate { get; set; }
        [DefaultValue("false")] public string AllowRemoval { get; set; }

        public string OrderByField { get; set; }

        private readonly bool _autoCommit;
        private readonly bool _hideExistingdata;
        private readonly string _listSchema;

        public CompositionCollectionProperties() {
            AllowInsertion = "true";
            AllowUpdate = "false";
            AllowRemoval = "false";
            _listSchema = "list";
            _autoCommit = true;
        }


        public CompositionCollectionProperties(string allowRemoval, string allowInsertion, string allowUpdate, string listSchema, bool autoCommit, bool hideExistingData, string orderByField,string prefilterFunction) {
            AllowRemoval = allowRemoval;
            AllowInsertion = allowInsertion;
            AllowUpdate = allowUpdate;
            _listSchema = listSchema;
            _autoCommit = autoCommit;
            _hideExistingdata = hideExistingData;
            OrderByField = orderByField;
            PrefilterFunction = prefilterFunction;
        }


        public string PrefilterFunction { get; set; }

        [DefaultValue("list")]
        public string ListSchema {
            get { return _listSchema; }
        }
        [DefaultValue(true)]
        public bool AutoCommit {
            get { return _autoCommit; }
        }

        public bool HideExistingData {
            get { return _hideExistingdata; }
        }

        public override string ToString() {
            return string.Format("AllowRemoval: {0}, AllowInsertion: {1}, " +
                                 "AllowUpdate: {2}, ListSchema: {3}", AllowRemoval, AllowInsertion, AllowUpdate, _listSchema);
        }
    }
}
