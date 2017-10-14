using System.Collections.Generic;

namespace softwrench.sW4.Shared2.Metadata.Menu.Interfaces {
    public interface IMenuAction {

        string Controller { get; set; }
        string Action { get; set; }
        IDictionary<string, object> Parameters { get; set; }
    }
}