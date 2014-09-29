using System;

namespace softwrench.sW4.Shared.Metadata.Applications.Relationships.Compositions {
    public class CompositionCollectionProperties {
        private readonly bool _allowRemoval;
        private readonly bool _allowInsertion;
        private readonly bool _allowUpdate;
        private readonly bool _autoCommit;
        private readonly String _listSchema;

        public CompositionCollectionProperties() {
            _allowInsertion = true;
            _allowUpdate = false;
            _allowRemoval = false;
            _listSchema = "list";
            _autoCommit = true;
        }

        public CompositionCollectionProperties(bool allowRemoval, bool allowInsertion, bool allowUpdate, string listSchema, bool autoCommit) {
            _allowRemoval = allowRemoval;
            _allowInsertion = allowInsertion;
            _allowUpdate = allowUpdate;
            _listSchema = listSchema;
            _autoCommit = autoCommit;
        }

        public bool AllowRemoval {
            get { return _allowRemoval; }
        }

        public bool AllowInsertion {
            get { return _allowInsertion; }
        }

        public bool AllowUpdate {
            get { return _allowUpdate; }
        }

        public string ListSchema {
            get { return _listSchema; }
        }

        public bool AutoCommit {
            get { return _autoCommit; }
        }

       

        public override string ToString() {
            return string.Format("AllowRemoval: {0}, AllowInsertion: {1}, " +
                                 "AllowUpdate: {2}, ListSchema: {3}", _allowRemoval, _allowInsertion, _allowUpdate, _listSchema);
        }
    }
}
