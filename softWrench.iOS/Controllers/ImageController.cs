using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Applications;

using softWrench.Mobile.Metadata.Applications.UI;
using softWrench.Mobile.Metadata.Extensions;
using softWrench.Mobile.Persistence;
using softWrench.Mobile.Persistence.Expressions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.iOS.Controllers
{
    public partial class ImageController : BaseController
	{
        private string _title;
        private string _image;

        public ImageController(IntPtr handle) : base (handle)
		{
		}

        public void Construct(ImageWidget widget, string title, string image)
        {
            if (widget == null) throw new ArgumentNullException("widget");
            if (title == null) throw new ArgumentNullException("title");
            if (image == null) throw new ArgumentNullException("image");

            _title = title;
            _image = image;

        }
      
        private void ShowImage()
        {
            var imageAsBase64 = _image;
            var imageAsByte = Convert.FromBase64String(imageAsBase64);
            var imageAsNSData = NSData.FromArray(imageAsByte);
                         
            imageView.Image = UIImage.LoadFromData(imageAsNSData);
        }

        private void AllowCloseImageByTapping()
        {
            UITapGestureRecognizer onTapDismissController = null;

            // Allow the controller to be dismissed by
            // just tapping the image. Convenient, huh?
            onTapDismissController = new UITapGestureRecognizer(_ =>
                {
                    imageView.RemoveGestureRecognizer(onTapDismissController);
                    onTapDismissController.Dispose();
                    onTapDismissController = null;

                    DismissViewController(true, null);
                });

            onTapDismissController.NumberOfTapsRequired = 1;
            imageView.AddGestureRecognizer(onTapDismissController);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ShowImage();
            AllowCloseImageByTapping();

            Title = string.IsNullOrWhiteSpace(_title) ? "Photo" : _title;
        }
    }
}
