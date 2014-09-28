using System;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.MobileCoreServices;
using MonoTouch.UIKit;

namespace softWrench.iOS.Utilities
{
    /// <summary>
    ///     Provides access to the device camera, taking
    ///     pictures or loading them from library.
    /// </summary>
    public static class Camera {
        static UIImagePickerController picker;
        static Action<NSDictionary> _callback;

        static void Init() {
            if (picker != null)
                return;

            picker = new UIImagePickerController();
            picker.Delegate = new CameraDelegate();
        }

        class CameraDelegate : UIImagePickerControllerDelegate {
            public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info) {
                var cb = _callback;
                _callback = null;

                picker.DismissViewController(true, () => cb(info));
            }
        }

        public static void TakePicture(UIViewController parent, Action<NSDictionary> callback) {
            Init();
            picker.SourceType = UIImagePickerControllerSourceType.Camera;
            _callback = callback;
            parent.PresentViewController(picker, true, () => Console.Write("Take"));
        }

        public static void SelectPicture(UIViewController parent, Action<NSDictionary> callback) {
            Init();
            picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            _callback = callback;
            parent.PresentViewController(picker, true,()=> Console.Write("Select"));
        }

        public static bool CanTakePicture() {
            return 
                UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera)
                && UIImagePickerController
                    .AvailableMediaTypes(UIImagePickerControllerSourceType.Camera)
                    .Any(m => m == UTType.Image);
        }
    }

}

