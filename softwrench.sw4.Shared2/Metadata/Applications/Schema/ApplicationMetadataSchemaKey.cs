using System;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema {

    /// <summary>
    /// The key which fully indentifies a schema to be renderized to the clients.
    /// </summary>
    public class ApplicationMetadataSchemaKey {

        public const string NotFoundPattern = "schema {0} not found";


        public ApplicationMetadataSchemaKey(string schemaId)
            : this(schemaId, (string)null, null) {
        }

        public InvalidOperationException NotFoundException() {
            return new InvalidOperationException(String.Format(NotFoundPattern, this));
        }

        public ApplicationMetadataSchemaKey() { }

        public ApplicationMetadataSchemaKey(string schemaId, SchemaMode? mode, ClientPlatform? platform) {
            SchemaId = schemaId;
            Mode = mode;
            Platform = platform;
        }

        public ApplicationMetadataSchemaKey(string schemaId, string mode, string platform) {
            SchemaId = schemaId;
            if (mode != null) {
                SchemaMode value;
                Enum.TryParse(mode, true, out value);
                Mode = value;
            }
            if (platform != null) {
                ClientPlatform value;
                Enum.TryParse(platform, true, out value);
                Platform = value;
            }
        }

        /// <summary>
        /// This was introduced on a later point, so, the api is not fully refactored across all places to use it
        /// </summary>
        public string ApplicationName { get; set; }

        public string SchemaId { get; set; }

        public SchemaMode? Mode { get; set; }

        public ClientPlatform? Platform { get; set; }


        protected bool Equals(ApplicationMetadataSchemaKey other) {
            var blankMode = Mode == null || other.Mode == null || Mode == SchemaMode.None || other.Mode == SchemaMode.None;
            return string.Equals(SchemaId, other.SchemaId,StringComparison.InvariantCultureIgnoreCase) && (blankMode || string.Equals(Mode, other.Mode)) &&
                (Platform == null || other.Platform == null || Platform == other.Platform);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ApplicationMetadataSchemaKey)obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = SchemaId?.ToLower().GetHashCode() ?? 0;
                return hashCode;
            }
        }

        public override string ToString() {
            return $"SchemaId: {SchemaId}, Mode: {Mode}, Platform: {Platform}";
        }

        public static ApplicationMetadataSchemaKey GetSyncInstance() {
            return new ApplicationMetadataSchemaKey(ApplicationMetadataConstants.SyncSchema, SchemaMode.None,
                ClientPlatform.Mobile);
        }


    }
}
