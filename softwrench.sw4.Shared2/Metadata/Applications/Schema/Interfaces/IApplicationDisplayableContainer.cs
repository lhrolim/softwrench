using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces {

    /// <summary>
    /// represents anything that can contain displayables, such as a schema a Tab or a Section
    /// </summary>
    public interface IApplicationDisplayableContainer {
        List<IApplicationDisplayable> Displayables { get; set; }

        string Id { get; }

    }
}