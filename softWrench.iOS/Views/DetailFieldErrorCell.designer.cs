using MonoTouch.Foundation;

namespace softWrench.iOS.Views
{
    [Register("DetailFieldErrorCell")]
    partial class DetailFieldErrorCell
    {
        [Outlet]
        MonoTouch.UIKit.UIImageView icon { get; set; }

        [Outlet]
        MonoTouch.UIKit.UILabel label { get; set; }

        [Outlet]
        MonoTouch.UIKit.UILabel message { get; set; }

        [Outlet]
        MonoTouch.UIKit.UITextField text { get; set; }
		
        void ReleaseDesignerOutlets ()
        {
            if (icon != null) {
                icon.Dispose ();
                icon = null;
            }

            if (label != null) {
                label.Dispose ();
                label = null;
            }

            if (message != null) {
                message.Dispose ();
                message = null;
            }

            if (text != null) {
                text.Dispose ();
                text = null;
            }
        }
    }
}