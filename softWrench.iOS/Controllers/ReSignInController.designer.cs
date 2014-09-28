// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace softWrench.iOS.Controllers
{
	[Register ("ReSignInController")]
	partial class ReSignInController
	{
		[Outlet]
		MonoTouch.UIKit.UILabel badCredentials { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton cancelButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIActivityIndicatorView indicator { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel invalidSettings { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel noConnectivity { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField password { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel sessionExpired { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton signInButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField username { get; set; }

		[Action ("Cancel:")]
		partial void Cancel (MonoTouch.UIKit.UIButton sender);

		[Action ("SignIn:")]
		partial void SignIn (MonoTouch.UIKit.UIButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (badCredentials != null) {
				badCredentials.Dispose ();
				badCredentials = null;
			}

			if (cancelButton != null) {
				cancelButton.Dispose ();
				cancelButton = null;
			}

			if (indicator != null) {
				indicator.Dispose ();
				indicator = null;
			}

			if (noConnectivity != null) {
				noConnectivity.Dispose ();
				noConnectivity = null;
			}

			if (password != null) {
				password.Dispose ();
				password = null;
			}

			if (sessionExpired != null) {
				sessionExpired.Dispose ();
				sessionExpired = null;
			}

			if (signInButton != null) {
				signInButton.Dispose ();
				signInButton = null;
			}

			if (username != null) {
				username.Dispose ();
				username = null;
			}

			if (invalidSettings != null) {
				invalidSettings.Dispose ();
				invalidSettings = null;
			}
		}
	}
}
