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
	[Register ("DetailImageCell")]
	partial class DetailImageCell
	{
		[Outlet]
		MonoTouch.UIKit.UIImageView image { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel label { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (label != null) {
				label.Dispose ();
				label = null;
			}

			if (image != null) {
				image.Dispose ();
				image = null;
			}
		}
	}
}
