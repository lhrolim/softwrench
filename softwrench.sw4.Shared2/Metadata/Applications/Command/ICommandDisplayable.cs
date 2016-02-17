using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace softwrench.sw4.Shared2.Metadata.Applications.Command {
    public interface ICommandDisplayable {
        String Id { get; }

        String Role { get; }

        String Position { get; }

        String Type { get; }

        String ShowExpression { get; }

        String PermissionExpression { get; }

        ICommandDisplayable KeepingOriginalData(ICommandDisplayable originalCommand);
    }
}
