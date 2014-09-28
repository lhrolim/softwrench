using System.Collections.Generic;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Behaviors
{
    internal class OnBeforeUploadContext : ApplicationBehaviorContext
    {
        public OnBeforeUploadContext(ApplicationSchemaDefinition application, MetadataRepository metadataRepository, User user)
            : base(application, metadataRepository, user)
        {
        }

        public override void Dispose()
        {
        }

        /// <summary>
        ///     Gets the object to serialized (typically
        ///     to Json) and then uploaded to the server.
        /// </summary>
        public IDictionary<string, object> Content { get; set; }
    }
}