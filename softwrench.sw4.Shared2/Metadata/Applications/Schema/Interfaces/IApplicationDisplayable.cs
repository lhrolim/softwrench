using System;
using System.ComponentModel;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces {
    /// <summary>
    /// Marking interface that all the items that can be displayed in an applicaiton should implement.
    /// </summary>
    public interface IApplicationDisplayable {

        string ToString();

        string RendererType { get; }

        string Type { get; }

        string Role { get; }

        [DefaultValue("true")]
        string ShowExpression { get; set; }

        string ToolTip { get; }

        [DefaultValue("")]
        string Label { get; }

        bool? ReadOnly { get; set; }
    }
}