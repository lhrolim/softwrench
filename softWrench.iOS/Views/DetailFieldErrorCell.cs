using System;
using MonoTouch.UIKit;
using softWrench.iOS.UI.Binding;
using softWrench.iOS.Utilities;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.iOS.Views
{
    public partial class DetailFieldErrorCell : DetailFieldCellBase
    {
        public DetailFieldErrorCell (IntPtr handle) : base (handle)
        {
        }

        internal override void Construct(ApplicationFieldDefinition metadata, ValueProvider valueProvider)
        {	        
            base.Construct(metadata, valueProvider);

            icon.Image = Theme.ErrorIcon;
        }

        public string Message
        {
            get { return message.Text; }
            set { message.Text = value; }
        }

        protected override UILabel Label
        {
            get { return label; }
        }

        protected override IUITextInput Text
        {
            get { return (IUITextInput) text; }
        }
    }
}