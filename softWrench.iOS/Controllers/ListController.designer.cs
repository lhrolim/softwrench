// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace softWrench.iOS.Controllers
{
	[Register ("ListController")]
	partial class ListController
	{
		[Outlet]
		MonoTouch.UIKit.UIBarButtonItem newButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView tableView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (newButton != null) {
				newButton.Dispose ();
				newButton = null;
			}

			if (tableView != null) {
				tableView.Dispose ();
				tableView = null;
			}
		}
	}
}
