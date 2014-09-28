// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace softWrench.iOS.Controllers
{
	[Register ("DetailComponentController")]
	partial class DetailComponentController
	{
		[Outlet]
		MonoTouch.UIKit.UIBarButtonItem cancelButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView containerView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIBarButtonItem doneButton { get; set; }

		[Action ("Cancel:")]
		partial void Cancel (MonoTouch.UIKit.UIBarButtonItem sender);

		[Action ("Done:")]
		partial void Done (MonoTouch.UIKit.UIBarButtonItem sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (containerView != null) {
				containerView.Dispose ();
				containerView = null;
			}

			if (cancelButton != null) {
				cancelButton.Dispose ();
				cancelButton = null;
			}

			if (doneButton != null) {
				doneButton.Dispose ();
				doneButton = null;
			}
		}
	}
}
