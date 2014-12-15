using System;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions {
    public class CompositionCollectionProperties {
        public string AllowRemoval { get; set; }
        public string AllowInsertion { get; set; }
        public string AllowUpdate { get; set; }

        public string OrderByField { get; set; }

        private readonly bool _autoCommit;
        private readonly bool _hideExistingdata;
        private readonly String _listSchema;

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


        public string ListSchema {
            get { return _listSchema; }
        }

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
