using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace softwrench.sw4.Shared2.Metadata.Applications.Command {
    public class RemoveCommand : ICommandDisplayable {
        public string Id {
            get; set;
        }
        public string Role {
            get; set;
        }
        public string Position {
            get; private set;
        }

        public string Type {
            get {
                return typeof(RemoveCommand).Name;
            }
        }

        public string ShowExpression {
            get; set;
        }

        public string PermissionExpression {
            get; set;
        }

        public string Label { get; set; }

        public ICommandDisplayable KeepingOriginalData(ICommandDisplayable originalCommand) {
            return this;
        }

        public RemoveCommand(string id) {
            Id = id;
        }

        protected bool Equals(RemoveCommand other) {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RemoveCommand)obj);
        }

        public override int GetHashCode() {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}
