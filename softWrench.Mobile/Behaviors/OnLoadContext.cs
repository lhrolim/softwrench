using softWrench.Mobile.Metadata;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Behaviors
{
    internal class OnLoadContext : ApplicationBehaviorContext
    {
        public OnLoadContext(ApplicationSchemaDefinition application, MetadataRepository metadataRepository, User user)
            : base(application, metadataRepository, user)
        {
        }

        public override void Dispose()
        {
        }
    }
}