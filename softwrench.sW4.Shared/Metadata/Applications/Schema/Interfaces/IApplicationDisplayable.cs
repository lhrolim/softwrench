namespace softwrench.sW4.Shared.Metadata.Applications.Schema.Interfaces {
    /// <summary>
    /// Marking interface that all the items that can be displayed in an applicaiton should implement.
    /// </summary>
    public interface IApplicationDisplayable {

        string ToString();

        string RendererType { get; }

        string Type { get; }
        string Role { get; }

        string ShowExpression { get; }

        string ToolTip { get; }
    }
}