using System;

namespace softwrench.sW4.Shared.Metadata.Applications.Schema {

    /// <summary>
    /// The key which fully indentifies a schema to be renderized to the clients.
    /// </summary>
    public class ApplicationMetadataSchemaKey
    {

        public const string NotFoundPattern = "schema {0} not found";
        

        private string _schemaId;
        private SchemaMode? _mode;
        private ClientPlatform? _platform;

        public ApplicationMetadataSchemaKey(string schemaId)
            : this(schemaId, (string)null, null) {
       }

        public InvalidOperationException NotFoundException()
        {
            return new InvalidOperationException(String.Format(NotFoundPattern,this));
        }

        public ApplicationMetadataSchemaKey() { }

        public ApplicationMetadataSchemaKey(string schemaId, SchemaMode? mode, ClientPlatform? platform) {
            _schemaId = schemaId;
            _mode = mode;
            _platform = platform;
        }

        public ApplicationMetadataSchemaKey(string schemaId, string mode, string platform) {
            _schemaId = schemaId;
            if (mode != null) {
                SchemaMode value;
                Enum.TryParse(mode, true, out value);
                _mode = value;
            }
            if (platform != null) {
                ClientPlatform value;
                Enum.TryParse(platform, true, out value);
                _platform = value;
            }
        }

        public string SchemaId {
            get { return _schemaId; }
            set { _schemaId = value; }
        }

        public SchemaMode? Mode {
            get { return _mode; }
            set { _mode = value; }
        }

        public ClientPlatform? Platform {
            get { return _platform; }
            set { _platform = value; }
        }


        protected bool Equals(ApplicationMetadataSchemaKey other) {
            var blankMode = _mode == null || other._mode == null || Mode == SchemaMode.None || other._mode == SchemaMode.None;
            return string.Equals(_schemaId, other._schemaId) && (blankMode || string.Equals(_mode, other._mode)) &&
                (_platform == null || other.Platform==null ||  _platform == other._platform);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationMetadataSchemaKey)obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (_schemaId != null ? _schemaId.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString() {
            return string.Format("Id: {0}, Mode: {1}, Platform: {2}", _schemaId, _mode, _platform);
        }


    }
}
