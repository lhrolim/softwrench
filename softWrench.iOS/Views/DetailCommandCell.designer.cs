// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace softWrench.iOS.Views
{
	[Register ("DetailCommandCell")]
	partial class DetailCommandCell
	{
		[Outlet]
		MonoTouch.UIKit.UIButton button { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel subtitle { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel title { get; set; }

		[Action ("ButtonTouchUpInside:")]
		partial void ButtonTouchUpInside (MonoTouch.UIKit.UIButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (button != null) {
				button.Dispose ();
				button = null;
			}

			if (subtitle != null) {
				subtitle.Dispose ();
				subtitle = null;
			}

			if (title != null) {
				title.Dispose ();
				title = null;
			}
		}
	}
}
