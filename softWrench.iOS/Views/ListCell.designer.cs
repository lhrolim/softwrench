// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace softWrench.iOS.Views
{
	[Register ("ListCell")]
	partial class ListCell
	{
		[Outlet]
		MonoTouch.UIKit.UIImageView bounce { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel excerpt { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel featured { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView featuredBackground { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel subtitle { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel title { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (excerpt != null) {
				excerpt.Dispose ();
				excerpt = null;
			}

			if (featured != null) {
				featured.Dispose ();
				featured = null;
			}

			if (featuredBackground != null) {
				featuredBackground.Dispose ();
				featuredBackground = null;
			}

			if (subtitle != null) {
				subtitle.Dispose ();
				subtitle = null;
			}

			if (title != null) {
				title.Dispose ();
				title = null;
			}

			if (bounce != null) {
				bounce.Dispose ();
				bounce = null;
			}
		}
	}
}
