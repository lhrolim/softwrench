using System;
using MonoTouch.UIKit;

namespace softWrench.iOS.Utilities
{
    public static class Alert
    {
        private static void ShowImpl(string title, string message, string cancelButtonTitle)
        {
            using (var alert = new UIAlertView(title, message, null, cancelButtonTitle))
            {
                alert.Show();
            }
        }

        public static void Show(string title, string message, string cancelButtonTitle = "OK", bool explicitlyInvokeOnMainThread = false)
        {
            if (title == null) throw new ArgumentNullException("title");

            // If the caller explicitly demanded
            // a main thread, let's wrap the call
            // inside an InvokeOnMainThread()
            if (explicitlyInvokeOnMainThread)
            {
                UIApplication
                    .SharedApplication
                    .InvokeOnMainThread(() => ShowImpl(title, message, cancelButtonTitle));

                return;
            }

            ShowImpl(title, message, cancelButtonTitle);
        }
    }
}