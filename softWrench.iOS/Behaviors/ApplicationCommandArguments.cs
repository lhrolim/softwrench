using MonoTouch.UIKit;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.iOS.Behaviors
{
    internal class ApplicationCommandArguments : IApplicationCommandArguments
    {
        private readonly ApplicationSchemaDefinition _applicationMetadata;
        private readonly MetadataRepository _metadataRepository;
        private readonly DataRepository _dataRepository;
        private readonly CompositeDataMap _composite;
        private readonly DataMap _dataMap;
        private readonly User _user;
        private readonly UIViewController _controller;

        public ApplicationCommandArguments(ApplicationSchemaDefinition schema, MetadataRepository metadataRepository, DataMap dataMap, DataRepository dataRepository, CompositeDataMap composite, User user, UIViewController controller)
        {
            _applicationMetadata = schema;
            _metadataRepository = metadataRepository;
            _dataMap = dataMap;
            _dataRepository = dataRepository;
            _composite = composite;
            _user = user;
            _controller = controller;
        }

        public ApplicationSchemaDefinition ApplicationSchemaDefinition
        {
            get { return _applicationMetadata; }
        }

        public DataMap DataMap
        {
            get { return _dataMap; }
        }

        public MetadataRepository MetadataRepository
        {
            get { return _metadataRepository; }
        }

        public DataRepository DataRepository
        {
            get { return _dataRepository; }
        }

        public CompositeDataMap Composite
        {
            get { return _composite; }
        }

        public User User
        {
            get { return _user; }
        }

        public UIViewController Controller
        {
            get { return _controller; }
        }
    }
}