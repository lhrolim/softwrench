// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace softWrench.iOS.Views
{
	[Register ("DetailFieldLargeTextErrorCell")]
	partial class DetailFieldLargeTextErrorCell
	{
		[Outlet]
		MonoTouch.UIKit.UIImageView icon { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel label { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel message { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextView text { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (icon != null) {
				icon.Dispose ();
				icon = null;
			}

			if (label != null) {
				label.Dispose ();
				label = null;
			}

			if (message != null) {
				message.Dispose ();
				message = null;
			}

			if (text != null) {
				text.Dispose ();
				text = null;
			}
		}
	}
}
