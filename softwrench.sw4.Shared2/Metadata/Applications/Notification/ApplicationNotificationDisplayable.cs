using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sw4.Shared2.Metadata.Applications.Notification {
    public class ApplicationNotificationDisplayable : IApplicationAttributeDisplayable {

        public ApplicationNotificationDisplayable(string label, string attribute)
        {
            Label = label;
            Attribute = attribute;
        }

        public string RendererType
        {
            get { throw new NotImplementedException(); }
        }

        public string Type
        {
            get { throw new NotImplementedException(); }
        }

        public string Role
        {
            get { throw new NotImplementedException(); }
        }

        public string ShowExpression
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string ToolTip
        {
            get { throw new NotImplementedException(); }
        }

        public string Attribute { get; set; }

        public string Label { get; set; }

        public string Qualifier
        {
            get { throw new NotImplementedException(); }
        }

        public bool? ReadOnly
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
