// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace softWrench.iOS.Controllers
{
	[Register ("DetailController")]
	partial class DetailController
	{
		[Outlet]
		MonoTouch.UIKit.UILabel bounceReason { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView containerView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton newCompositionButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton newItemButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UISegmentedControl segmentedControl { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (newCompositionButton != null) {
				newCompositionButton.Dispose ();
				newCompositionButton = null;
			}

			if (bounceReason != null) {
				bounceReason.Dispose ();
				bounceReason = null;
			}

			if (containerView != null) {
				containerView.Dispose ();
				containerView = null;
			}

			if (newItemButton != null) {
				newItemButton.Dispose ();
				newItemButton = null;
			}

			if (segmentedControl != null) {
				segmentedControl.Dispose ();
				segmentedControl = null;
			}
		}
	}
}
