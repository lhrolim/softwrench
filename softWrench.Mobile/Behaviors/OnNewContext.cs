using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Behaviors
{
    internal class OnNewContext : ApplicationBehaviorContext
    {
        private readonly CompositeDataMap _composite;

        public OnNewContext(ApplicationSchemaDefinition application, MetadataRepository metadataRepository, User user, CompositeDataMap composite)
            : base(application, metadataRepository, user)
        {
            _composite = composite;
        }

        public CompositeDataMap Composite
        {
            get { return _composite; }
        }

        public override void Dispose()
        {
        }
    }
}