using System;
using System.Collections.Generic;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces {
    /// <summary>
    /// Marking interface that all the items that can be displayed in an applicaiton should implement.
    /// </summary>
    public interface IApplicationDisplayable {

        string ToString();

        string RendererType { get; }

        IDictionary<string, string> RendererParameters { get; }

        string Type { get; }
        string Role { get; }

        string ShowExpression { get; set; }

        string EnableExpression {get; set;}

        string ToolTip { get; }

        bool? ReadOnly { get; set; }

    }
}