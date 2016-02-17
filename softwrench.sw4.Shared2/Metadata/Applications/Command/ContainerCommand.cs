using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sW4.Shared2.Metadata.Applications.Command;

namespace softwrench.sw4.Shared2.Metadata.Applications.Command {

    public class ContainerCommand : ICommandDisplayable {




        public string Id {
            get; set;
        }
        public string Role {
            get; set;
        }
        public string Position {
            get; set;
        }

        public string Label {
            get; set;
        }
        public string Tooltip {
            get; set;
        }
        public string Icon {
            get; set;
        }
        public string Service {
            get; set;
        }
        public string Method {
            get; set;
        }

        public string ShowExpression {
            get; set;
        }

        public string PermissionExpression {
            get; set;
        }

        public IEnumerable<ICommandDisplayable> Displayables {
            get; set;
        }

        public string Type {
            get {
                return typeof(ContainerCommand).Name;
            }
        }
        public ICommandDisplayable KeepingOriginalData(ICommandDisplayable originalCommand) {
            return this;
        }

        public ContainerCommand(string id, string label, string tooltip, string role, string position, string icon, string service, string method, IEnumerable<ICommandDisplayable> displayables, string permissionexpression) {
            Id = id;
            Label = label;
            Tooltip = tooltip;
            Displayables = displayables;
            Role = role;
            Position = position;
            Icon = icon;
            Service = service;
            Method = method;
            PermissionExpression = permissionexpression;
        }

        public bool IsDynamic() {

            var result = false;
            foreach (var commandDisplayable in Displayables) {
                if (commandDisplayable is ContainerCommand) {
                    result = result || ((ContainerCommand)commandDisplayable).IsDynamic();
                }
                if (commandDisplayable.Role != null || commandDisplayable.PermissionExpression != null) {
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

        protected bool Equals(ContainerCommand other) {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContainerCommand)obj);
        }

        public override int GetHashCode() {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}
