using System;
using MonoTouch.UIKit;

namespace softWrench.iOS.Views
{
    public partial class DetailFieldLargeTextCell : DetailFieldCellBase
	{
        public DetailFieldLargeTextCell(IntPtr handle)
            : base(handle)
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
