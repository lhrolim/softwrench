using System.Collections.Generic;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Behaviors
{
    internal class OnBeforeShowContext : ApplicationBehaviorContext
    {
        private readonly IDictionary<string, IApplicationCommand> _commands;

        public OnBeforeShowContext(ApplicationSchemaDefinition application, MetadataRepository metadataRepository, User user)
            : base(application, metadataRepository, user)
        {
            _commands = new Dictionary<string, IApplicationCommand>();
        }

        public IEnumerable<IApplicationCommand> Commands
        {
            get { return _commands.Values; }
        }

        public void RegisterCommand(IApplicationCommand command)
        {
            _commands[command.Name] = command;
        }

        public override void Dispose()
        {
            foreach (var command in _commands)
            {
                command.Value.Dispose();
            }
        }
    }
}