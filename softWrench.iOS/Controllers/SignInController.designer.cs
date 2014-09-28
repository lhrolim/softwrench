// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace softWrench.iOS.Controllers
{
	[Register ("SignInController")]
	partial class SignInController
	{
		[Outlet]
		MonoTouch.UIKit.UILabel badCredentials { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView container { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIActivityIndicatorView indicator { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel invalidSettings { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView logo { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel noConnectivity { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField password { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton signInButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView splash { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField username { get; set; }

		[Action ("Settings:")]
		partial void Settings (MonoTouch.UIKit.UIButton sender);

		[Action ("SignIn:")]
		partial void SignIn (MonoTouch.UIKit.UIButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (badCredentials != null) {
				badCredentials.Dispose ();
				badCredentials = null;
			}

			if (container != null) {
				container.Dispose ();
				container = null;
			}

			if (indicator != null) {
				indicator.Dispose ();
				indicator = null;
			}

			if (logo != null) {
				logo.Dispose ();
				logo = null;
			}

			if (noConnectivity != null) {
				noConnectivity.Dispose ();
				noConnectivity = null;
			}

			if (invalidSettings != null) {
				invalidSettings.Dispose ();
				invalidSettings = null;
			}

			if (password != null) {
				password.Dispose ();
				password = null;
			}

			if (signInButton != null) {
				signInButton.Dispose ();
				signInButton = null;
			}

			if (splash != null) {
				splash.Dispose ();
				splash = null;
			}

			if (username != null) {
				username.Dispose ();
				username = null;
			}
		}
	}
}
