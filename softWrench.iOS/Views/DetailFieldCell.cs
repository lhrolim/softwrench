using System;
using MonoTouch.UIKit;

namespace softWrench.iOS.Views
{
    public partial class DetailFieldCell : DetailFieldCellBase
	{
	    public DetailFieldCell (IntPtr handle) : base(handle)
		{
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
