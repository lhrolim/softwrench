using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sw4.Shared2.Metadata.Applications.Schema {
    public class TreeDefinition: IApplicationIndentifiedDisplayable {
        public TreeDefinition() {
        }

        public TreeDefinition(string attribute, string label, string tootip, string showExpression, string listtype, string startIndex, List<TreeNode> nodes, List<IApplicationDisplayable> fields) {
            Attribute = attribute;
            Label = label;
            ToolTip = tootip;
            ShowExpression = showExpression;
            RendererType = null;
            Type = GetType().Name;
            Role = Attribute;
            ListType = listtype;
            StartIndex = startIndex;
            Nodes = nodes;
            Fields = fields;
        }

        public string RendererType { get; set; }
        public string Type { get; set; }
        public string ShowExpression { get; set; }
        public string ToolTip { get; set; }
        public string Label { get; set; }
        public bool IsReadOnly { get { return false; } set { } }
        public string Attribute { get; set; }
        public string Role { get; set; }
        public string ListType { get; set; }
        public string StartIndex { get; set; }
        public List<TreeNode> Nodes { get; set; }
        public List<IApplicationDisplayable> Fields { get; set; }
    }
}
