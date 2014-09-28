using System;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Behaviors
{
    internal abstract class ApplicationBehaviorContext : IDisposable
    {
        private readonly ApplicationSchemaDefinition _application;
        private readonly MetadataRepository _metadataRepository;
        private readonly User _user;

        protected ApplicationBehaviorContext(ApplicationSchemaDefinition application, MetadataRepository metadataRepository, User user)
        {
            if (application == null) throw new ArgumentNullException("application");
            if (metadataRepository == null) throw new ArgumentNullException("metadataRepository");
            if (user == null) throw new ArgumentNullException("user");

            _application = application;
            _metadataRepository = metadataRepository;
            _user = user;
        }

        public ApplicationSchemaDefinition Application
        {
            get { return _application; }
        }

        public MetadataRepository MetadataRepository
        {
            get { return _metadataRepository; }
        }

        public User User
        {
            get { return _user; }
        }

        public abstract void Dispose();
    }
}