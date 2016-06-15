using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using log4net;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata.Applications.Command;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;

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

        private static IList<ActionPermission> LocateActionPermissions(MergedUserProfile profile, string barKey) {
            if (barKey.GetNumberOfItems("_") < 2 ) {
                return new List<ActionPermission>();
            }
            var applicationFinalDelimiter = 1;
            if (barKey.StartsWith("_")) {
                //swdb applications start with _
                applicationFinalDelimiter = 2;
            }

            //format: application_schema_mode#barid
            var applicationIdx = barKey.GetNthIndex('_', applicationFinalDelimiter);
            var modeIdx = barKey.GetNthIndex('_', applicationFinalDelimiter + 1);
            var applicationName = barKey.Substring(0, applicationIdx);
            var schemaName = barKey.Substring(applicationIdx + 1, modeIdx - (applicationIdx + 1));
            var applicationPermission = profile.GetPermissionByApplication(applicationName);
            if (applicationPermission == null) {
                return new List<ActionPermission>();
            }
            return new List<ActionPermission>(applicationPermission.ActionPermissions.Where(s => s.Schema.EqualsIc(schemaName)));
        }


        public static IDictionary<string, CommandBarDefinition> SecuredBars(InMemoryUser user, IDictionary<string, CommandBarDefinition> commandBars) {
            var result = new Dictionary<string, CommandBarDefinition>();
            foreach (var commandBar in commandBars) {

                var commandbarKey = commandBar.Key;
                var permissions = LocateActionPermissions(user.MergedUserProfile, commandbarKey);

                var commandBarDefinition = commandBar.Value;
                if (!commandBarDefinition.IsDynamic() && !permissions.Any()) {
                    //if there are no roles, simply add it. this will also trigger a cache on the bar for the next user
                    result[commandBar.Key] = commandBarDefinition;
                    Log.DebugFormat("returning bar {0} due to abscence of roles on it", commandBar.Key);
                    continue;
                }
                var commands = new List<ICommandDisplayable>();

                foreach (var command in commandBarDefinition.Commands) {
                    if (command is ContainerCommand) {
                        var secured = ((ContainerCommand)command).Secure(user, permissions);
                        if (secured != null) {
                            commands.Add(secured);
                        }
                    } else {
                        if (!command.IsMetadataPermitted()) {
                            Log.DebugFormat("ignoring command {0} due to metadata restriction", command.Id);
                        } else if (user.IsSwAdmin()) {
                            //sw admin sees it all
                            commands.Add(command);
                        } else if (!command.Permitted(user, permissions)) {
                            Log.DebugFormat("ignoring command {0} due to abscence of role {1}", command.Id, command.Role);
                        } else {
                            commands.Add(command);
                        }

                    }
                }
                result[commandBar.Key] = new CommandBarDefinition(commandBarDefinition.Id, commandBarDefinition.Position, commandBarDefinition.ExcludeUndeclared, commands);
            }
            return result;
        }
    }
}
