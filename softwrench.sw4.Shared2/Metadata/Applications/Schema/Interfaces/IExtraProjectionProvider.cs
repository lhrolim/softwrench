using System.Collections.Generic;

namespace softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces {
    public interface IExtraProjectionProvider {
        ISet<string> ExtraProjectionFields { get; set; }
    }
}
