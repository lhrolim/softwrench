using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sW4.Shared2.Metadata.Applications.Command;

namespace softwrench.sw4.Shared2.Metadata.Applications.Command {

    public class ContainerCommand : ICommandDisplayable {




        public string Id { get; set; }
        public string Role { get; set; }
        public string Position { get; set; }

        public string Label { get; set; }
        public string Tooltip { get; set; }


        public IEnumerable<ICommandDisplayable> Displayables { get; set; }

        public string Type { get { return typeof(ContainerCommand).Name; } }

        public ContainerCommand(string id, string label, string tooltip, string role, string position, IEnumerable<ICommandDisplayable> displayables) {
            Id = id;
            Label = label;
            Tooltip = tooltip;
            Displayables = displayables;
            Role = role;
            Position = position;
        }

        public bool IsDynamic() {

            var result = false;
            foreach (var commandDisplayable in Displayables) {
                if (commandDisplayable is ContainerCommand) {
                    result = result || ((ContainerCommand)commandDisplayable).IsDynamic();
                }
                if (commandDisplayable.Role != null) {
                    return true;
                }
            }
            return result;
        }

        public ICommandDisplayable FindById(string id) {
            foreach (var command in Displayables) {
                if (command is ContainerCommand) {
                    var result = ((ContainerCommand)command).FindById(id);
                    if (result != null) {
                        return result;
                    }
                } else if (command.Id == id) {
                    return command;
                }
            }
            return null;
        }

      
    }
}
