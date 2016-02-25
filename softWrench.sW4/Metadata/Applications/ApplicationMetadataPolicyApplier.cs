using System;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Metadata.Applications {
    public class ApplicationMetadataPolicyApplier {
        private readonly Lazy<ApplicationMetadata> _result;
        private readonly CompleteApplicationMetadataDefinition _application;
        private readonly ApplicationMetadataSchemaKey _schemaKey;
        private readonly InMemoryUser _user;
        private readonly ClientPlatform _platform;
        private readonly string _schemaFieldsToDisplay;


        public ApplicationMetadataPolicyApplier([NotNull] CompleteApplicationMetadataDefinition application, ApplicationMetadataSchemaKey schemaKey, [NotNull] InMemoryUser user, ClientPlatform platform, string schemaFieldsToDisplay) {
            if (application == null) throw new ArgumentNullException("application");
            if (user == null) throw new ArgumentNullException("user");

            _application = application;
            _user = user;
            _schemaKey = schemaKey;
            _platform = platform;
            _schemaFieldsToDisplay = schemaFieldsToDisplay;
            _result = new Lazy<ApplicationMetadata>(ApplyImpl);
        }

        private ApplicationMetadata ApplyImpl() {
            var schema = _application
                .SchemaForPlatform(_schemaKey);
            var securedSchema = schema.ApplyPolicy(_user.Roles, _platform,_schemaFieldsToDisplay, _user.MergedUserProfile);

            return ApplicationMetadata.CloneSecuring(_application, securedSchema);

        }

        public ApplicationMetadata Apply() {
            return _result.Value;
        }
    }
}
