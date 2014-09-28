using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.iOS.UI.Binding;

namespace softWrench.iOS.Views
{
    public partial class DetailImageCell : DetailFieldCellBase
	{
        private readonly IUITextInput _text;

        public DetailImageCell (IntPtr handle) : base(handle)
		{
            _text = new VirtualTextInput(this);
		}

        protected override UILabel Label
        {
            get { return label; }
        }

        protected override IUITextInput Text
        {
            get { return _text; }
        }

        private class VirtualTextInput : IUITextInput
        {
            private readonly DetailImageCell _cell;
            private string _imageBase64;

            public VirtualTextInput(DetailImageCell cell)
            {
                _cell = cell;
            }

            private void LoadImageFromBase64(UIImageView view, string base64)
            {
                if (_imageBase64 == base64)
                {
                    return;
                }

                if (string.IsNullOrEmpty(base64))
                {
                    view.Image = null;
                    return;
                }

                var imageAsByte = Convert.FromBase64String(base64);
                var imageAsNSData = NSData.FromArray(imageAsByte);
                view.Image = UIImage.LoadFromData(imageAsNSData);
            }

            public bool ResignFirstResponder()
            {
                return true;
            }

            public string Text
            {
                get
                {
                    return _imageBase64;
                }
                set
                {
                    LoadImageFromBase64(_cell.image, value);             
                    _imageBase64 = value;
                }
            }

            public UIKeyboardType KeyboardType { get; set; }
            public UITextFieldCondition ShouldReturn { get; set; }
            public UIColor TextColor { get; set; }       
            public bool UserInteractionEnabled { get; set; }
        }
	}
}
