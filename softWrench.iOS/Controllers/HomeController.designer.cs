// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace softWrench.iOS.Controllers
{
	[Register ("HomeController")]
	partial class HomeController
	{
		[Outlet]
		MonoTouch.UIKit.UIButton synchronizeButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel synchronizeSubtitle { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel synchronizeTitle { get; set; }

		[Action ("Synchronize:")]
		partial void Synchronize (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (synchronizeTitle != null) {
				synchronizeTitle.Dispose ();
				synchronizeTitle = null;
			}

			if (synchronizeSubtitle != null) {
				synchronizeSubtitle.Dispose ();
				synchronizeSubtitle = null;
			}

			if (synchronizeButton != null) {
				synchronizeButton.Dispose ();
				synchronizeButton = null;
			}
		}
	}
}
