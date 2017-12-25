using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sw4.Shared2.Metadata.Applications.Schema {
    public class TableDefinition : IApplicationIndentifiedDisplayable {
        public TableDefinition() {
        }

        public TableDefinition(string attribute, string label, string tootip, string showExpression, List<string> headers, List<List<IApplicationDisplayable>> rows) {
            Attribute = attribute;
            Label = label;
            ToolTip = tootip;
            ShowExpression = showExpression;
            Headers = headers;
            Rows = rows;
            RendererType = null;
            Type = GetType().Name;
            Role = Attribute;
        }

        public string RendererType { get; set; }
        public string Type { get; set; }
        public string ShowExpression { get; set; }
        public string ToolTip { get; set; }
        public string Label { get; set; }
        public bool IsReadOnly { get { return false; } set { } }
        public string Attribute { get; set; }
        public string Role { get; set; }
        public List<List<IApplicationDisplayable>> Rows { get; set; }
        public List<string> Headers { get; set; }
    }
}
