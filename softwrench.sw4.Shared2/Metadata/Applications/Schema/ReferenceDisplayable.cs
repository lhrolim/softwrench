using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.portable.Util;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Util;

namespace softwrench.sw4.Shared2.Metadata.Applications.Schema {

    /// <summary>
    /// This displayable should reference another one contained into the Application root Declaration
    /// </summary>
    public class ReferenceDisplayable : IApplicationDisplayable {

        public string Id { get; set; }

        public string ShowExpression { get; set; }

        public string Attribute { get; set; }
        public string Label { get; set; }

        public bool IsReadOnly { get; set; }

        public string EnableExpression { get; set; }

        public IDictionary<string, object> Properties { get; set; }

        public string PropertiesString {
            set { Properties = PropertyUtil.ConvertToDictionary(value); }
        }

        public override string ToString() {
            return string.Format("Reference-- Id: {0}", Id);
        }

        #region uselessMethodsForThisDisplayable

        public string RendererType { get; set; }
        public string Type { get; set; }
        public string Role { get; set; }

        public string ToolTip { get; set; }

        #endregion
    }
}
