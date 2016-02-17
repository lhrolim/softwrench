using System.Collections.Generic;
using JetBrains.Annotations;
using log4net;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata.Applications.Command;
using softwrench.sw4.Shared2.Metadata.Applications.Command;

namespace softWrench.sW4.Metadata.Applications.Command {
    public class ApplicationCommandUtils {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ApplicationCommandUtils));

        [CanBeNull]
        public static ApplicationCommand GetApplicationCommand(ApplicationMetadata applicationMetadata, [CanBeNull]string commandId) {
            if (commandId == null) {
                return null;
            }
            var applicationCommands = applicationMetadata.Schema.CommandSchema.ApplicationCommands;
            var commandParts = commandId.Split('.');
            var barKey = commandParts[0];
            if (!applicationCommands.ContainsKey(barKey)) {
                return MetadataProvider.Command(commandId) as ApplicationCommand;
            }
            var commandBar = applicationCommands[barKey];
            var commanddisplayableId = commandParts[1];
            var overridenCommand = commandBar.FindById(commanddisplayableId);
            if (overridenCommand == null) {
                return MetadataProvider.Command(commandId) as ApplicationCommand;
            }
            return overridenCommand as ApplicationCommand;
        }

        public static IDictionary<string, CommandBarDefinition> SecuredBars(InMemoryUser user, IDictionary<string, CommandBarDefinition> commandBars) {
            var result = new Dictionary<string, CommandBarDefinition>();
            foreach (var commandBar in commandBars) {
                var commandBarDefinition = commandBar.Value;
                if (!commandBarDefinition.IsDynamic()) {
                    //if there are no roles, simply add it. this will also trigger a cache on the bar for the next user
                    result[commandBar.Key] = commandBarDefinition;
                    Log.DebugFormat("returning bar {0} due to abscence of roles on it", commandBar.Key);
                    continue;
                }
                var commands = new List<ICommandDisplayable>();

                foreach (var command in commandBarDefinition.Commands) {
                    if (command is ContainerCommand) {
                        var secured = ((ContainerCommand)command).Secure(user);
                        if (secured != null) {
                            commands.Add(secured);
                        }
                    } else {
                        if (!user.IsInRole(command.Role) && !user.IsSwAdmin()) {
                            //if not in role, just skip it (unless swadmin that can see everything...)
                            Log.DebugFormat("ignoring command {0} due to abscence of role {1}", command.Id, command.Role);
                            continue;
                        }
                        if (command.Hidden()) {
                            continue;
                        }
                        commands.Add(command);
                    }
                }
                result[commandBar.Key] = new CommandBarDefinition(commandBarDefinition.Id, commandBarDefinition.Position, commandBarDefinition.ExcludeUndeclared, commands);
            }
            return result;
        }
    }
}
