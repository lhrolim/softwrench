using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema {
    public class TableColumnPlaceHolder: IApplicationDisplayable {
        public TableColumnPlaceHolder() {
        }

        public TableColumnPlaceHolder(IDefaultValueApplicationDisplayable parent) {
            Attribute = parent.Attribute;
            Label = parent.Label;
            ToolTip = parent.ToolTip;
            ShowExpression = parent.ShowExpression;
            RendererType = null;
            Type = GetType().Name;
            Role = Attribute;
        }

        public string RendererType { get; set; }
        public string Type { get; set; }
        public string Role { get; set; }
        public string ShowExpression { get; set; }
        public string ToolTip { get; set; }
        public string Label { get; set; }
        public bool IsReadOnly { get; set; }
        public string Attribute { get; set; }

        public IDictionary<string, object> RendererParameters { get; set; }

        public int ParentIndex { get; set; }
        public int IndexOnParent { get; set; }
    }
}
