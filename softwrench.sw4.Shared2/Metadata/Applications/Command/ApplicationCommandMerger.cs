using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sw4.Shared2.Metadata.Exception;

namespace softwrench.sw4.Shared2.Metadata.Applications.Command {
    public class ApplicationCommandMerger {

        public static IDictionary<string, CommandBarDefinition> MergeCommandsWithCustomizedSchema(
            IDictionary<string, CommandBarDefinition> schemaCommands, IDictionary<string, CommandBarDefinition> originalCommandBars) {
            if (schemaCommands == null) {
                return originalCommandBars;
            }
            foreach (var barKey in schemaCommands.Keys) {
                if (!originalCommandBars.ContainsKey(barKey)) {
                    //non need to merge here
                    originalCommandBars[barKey] = schemaCommands[barKey];
                    continue;
                }
                var commandBar = originalCommandBars[barKey];
                var schemaBar = schemaCommands[barKey];
                originalCommandBars[barKey] = DoMergeBars(schemaBar, commandBar);
            }
            return originalCommandBars;
        }


        public static IDictionary<string, CommandBarDefinition> MergeCommands(
            IDictionary<string, CommandBarDefinition> schemaCommands, IDictionary<string, CommandBarDefinition> commandBars) {
            var result = new Dictionary<string, CommandBarDefinition>();
            if (schemaCommands == null) {
                return result;
            }
            foreach (var barKey in schemaCommands.Keys) {
                if (!commandBars.ContainsKey(barKey)) {
                    throw new MetadataException(
                        String.Format("Command bar {0} not found, review your metadata configuration", barKey));
                }
                var commandBar = commandBars[barKey];
                var schemaBar = schemaCommands[barKey];
                result[barKey] = DoMergeBars(schemaBar, commandBar);
            }



            return result;
        }

        public static CommandBarDefinition DoMergeBars(CommandBarDefinition overridingBar, CommandBarDefinition baseBar) {
            var listOfCommands = new HashSet<ICommandDisplayable>();
            foreach (var leftCommand in overridingBar.Commands.Where(s => "left".Equals(s.Position))) {
                listOfCommands.Add(leftCommand);
            }
            //although there are lots of loops here, the list of commands is small, so no need to eagerly optimize it
            foreach (var originalCommand in baseBar.Commands) {
                var command = originalCommand;
                foreach (var leftOfCommand in overridingBar.Commands.Where(s => IsLeftOfCommand(s, command))) {
                    listOfCommands.Add(leftOfCommand);
                }

                var overridenCommand = overridingBar.Commands.FirstOrDefault(c => c.Id == command.Id);
                if (overridenCommand == null) {
                    if (!overridingBar.ExcludeUndeclared) {
                        listOfCommands.Add(command);
                    }
                } else if (!(overridenCommand is RemoveCommand)) {
                    //replaces command unless marked to excluded; in that case we wont do nothing
                    listOfCommands.Add(overridenCommand.KeepingOriginalData(originalCommand));
                }
                foreach (var rightCommand in overridingBar.Commands.Where(s => IsRightOfCommand(s, command))) {
                    listOfCommands.Add(rightCommand);
                }
            }
            foreach (var rightCommand in overridingBar.Commands.Where(s => s.Position == null || "right".Equals(s.Position))) {
                if (listOfCommands.All(a => a.Id != rightCommand.Id) && !(rightCommand is RemoveCommand)) {
                    //TODO: remove this workaround
                    listOfCommands.Add(rightCommand);
                }
            }

            return new CommandBarDefinition(overridingBar.Id, baseBar.Position, baseBar.ExcludeUndeclared, listOfCommands);
        }

        private static bool IsLeftOfCommand(ICommandDisplayable commandDisplayable, ICommandDisplayable originalCommand) {
            return commandDisplayable.Position == "<" + originalCommand.Id || commandDisplayable.Position == "-" + originalCommand.Id;
        }

        private static bool IsRightOfCommand(ICommandDisplayable commandDisplayable, ICommandDisplayable originalCommand) {
            return commandDisplayable.Position == ">" + originalCommand.Id || commandDisplayable.Position == "+" + originalCommand.Id;
        }
    }
}
