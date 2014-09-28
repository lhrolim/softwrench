// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace softWrench.iOS.Controllers
{
	[Register ("SettingsController")]
	partial class SettingsController
	{
		[Outlet]
		MonoTouch.UIKit.UIButton saveButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField server { get; set; }

		[Action ("Save:")]
		partial void Save (MonoTouch.UIKit.UIBarButtonItem sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (server != null) {
				server.Dispose ();
				server = null;
			}

			if (saveButton != null) {
				saveButton.Dispose ();
				saveButton = null;
			}
		}
	}
}
