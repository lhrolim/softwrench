// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using MonoTouch.Foundation;

namespace softWrench.iOS.Views
{
    [Register ("DetailDatePickerCell")]
	partial class DetailDatePickerCell
	{
		[Outlet]
		MonoTouch.UIKit.UIDatePicker picker { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (picker != null) {
				picker.Dispose ();
				picker = null;
			}
		}
	}
}
