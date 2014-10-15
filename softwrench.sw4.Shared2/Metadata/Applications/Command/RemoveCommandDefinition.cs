using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace softwrench.sw4.Shared2.Metadata.Applications.Command {
    public class RemoveCommand : ICommandDisplayable {
        public string Id { get; set; }
        public string Role { get; set; }
        public string Position { get; private set; }

        public string Type { get { return typeof(RemoveCommand).Name; } }

        public RemoveCommand(string id) {
            Id = id;
        }
    }
}
