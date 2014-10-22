﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sw4.Shared2.Metadata.Exception;

namespace softwrench.sw4.Shared2.Metadata.Applications.Command {
    public class ApplicationCommandMerger {


        public static IDictionary<string, CommandBarDefinition> MergeCommands(
            IDictionary<string, CommandBarDefinition> schemaCommands, IDictionary<string, CommandBarDefinition> commandBars) {
            var result = new Dictionary<string, CommandBarDefinition>();
            if (schemaCommands == null) {
                return result;
            }
            foreach (var barKey in schemaCommands.Keys) {
                if (!commandBars.ContainsKey(barKey)) {
                    throw new MetadataException(String.Format("Command bar {0} not found, review your metadata configuration", barKey));
                }
                var commandBar = commandBars[barKey];
                var schemaBar = schemaCommands[barKey];
                var listOfCommands = new HashSet<ICommandDisplayable>();
                foreach (var leftCommand in schemaBar.Commands.Where(s => "left".Equals(s.Position))) {
                    listOfCommands.Add(leftCommand);
                }
                //although there are lots of loops here, the list of commands is small, so no need to eagerly optimize it
                foreach (var originalCommand in commandBar.Commands) {
                    var command = originalCommand;
                    foreach (var leftOfCommand in schemaBar.Commands.Where(s => IsLeftOfCommand(s, command))) {
                        listOfCommands.Add(leftOfCommand);
                    }

                    var overridenCommand = schemaBar.Commands.FirstOrDefault(c => c.Id == command.Id);
                    if (overridenCommand == null) {
                        if (!schemaBar.ExcludeUndeclared) {
                            listOfCommands.Add(command);
                        }
                    } else if (!(overridenCommand is RemoveCommand)) {
                        //replaces command unless marked to excluded; in that case we wont do nothing
                        listOfCommands.Add(overridenCommand);
                    }
                    foreach (var rightCommand in schemaBar.Commands.Where(s => IsRightOfCommand(s, command))) {
                        listOfCommands.Add(rightCommand);
                    }
                }
                foreach (var rightCommand in schemaBar.Commands.Where(s => s.Position == null || "right".Equals(s.Position))) {
                    if (listOfCommands.All(a => a.Id != rightCommand.Id) && !(rightCommand is RemoveCommand)) {
                        //TODO: remove this workaround
                        listOfCommands.Add(rightCommand);
                    }
                }

                result[barKey] = new CommandBarDefinition(barKey, commandBar.Position, commandBar.ExcludeUndeclared, listOfCommands);
            }
            return result;
        }

        private static bool IsLeftOfCommand(ICommandDisplayable commandDisplayable, ICommandDisplayable originalCommand) {
            return commandDisplayable.Position == "<" + originalCommand.Id;
        }

        private static bool IsRightOfCommand(ICommandDisplayable commandDisplayable, ICommandDisplayable originalCommand) {
            return commandDisplayable.Position == ">" + originalCommand.Id;
        }
    }
}
