// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace softWrench.iOS.Views
{
	[Register ("DetailFieldCell")]
	partial class DetailFieldCell
	{
		[Outlet]
		MonoTouch.UIKit.UILabel label { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField text { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (label != null) {
				label.Dispose ();
				label = null;
			}

			if (text != null) {
				text.Dispose ();
				text = null;
			}
		}
	}
}
