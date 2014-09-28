using System;
using MonoTouch.UIKit;
using softWrench.iOS.Utilities;

namespace softWrench.iOS.Controllers
{
	public abstract class BaseNavigationRootController : BaseController
	{
        private readonly NavigationRootControllerDelegate _delegate;


        public BaseNavigationRootController (IntPtr handle) : base (handle)
		{
            _delegate = new NavigationRootControllerDelegate(this);
		}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _delegate.Dispose();
            }

            base.Dispose(disposing);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _delegate.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _delegate.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            _delegate.ViewWillDisappear(animated);
        }

        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            base.WillRotate(toInterfaceOrientation, duration);
            _delegate.WillRotate(toInterfaceOrientation, duration);
        }
	}
}
