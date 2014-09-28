// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace softWrench.iOS.Controllers
{
	[Register ("ApplicationController")]
	partial class ApplicationController
	{
		[Outlet]
		MonoTouch.UIKit.UIView detailView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView masterView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (detailView != null) {
				detailView.Dispose ();
				detailView = null;
			}

			if (masterView != null) {
				masterView.Dispose ();
				masterView = null;
			}
		}
	}
}
