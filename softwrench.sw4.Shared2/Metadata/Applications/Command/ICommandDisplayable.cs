using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace softwrench.sw4.Shared2.Metadata.Applications.Command {
    public interface ICommandDisplayable {
        string Id { get; }

        string Role { get; }

        string Position { get; }

        string Type { get; }

        string ShowExpression { get; }

        string PermissionExpression { get; }

        string Label { get; }

        ICommandDisplayable KeepingOriginalData(ICommandDisplayable originalCommand);
    }
}
