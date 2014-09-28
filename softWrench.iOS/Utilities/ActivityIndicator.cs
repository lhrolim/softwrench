using System;
using System.Drawing;
using MonoTouch.UIKit;

namespace softWrench.iOS.Utilities
{
    public class ActivityIndicator : IDisposable
    {
        private static void Stop(ActivityIndicator activityIndicator)
        {
            activityIndicator.View.StopAnimating();

            UIView.Animate(0.25,
                () => activityIndicator.View.Alpha = 0,
                () =>
                {
                    if (false == activityIndicator.IsOwner)
                    {
                        activityIndicator.View.Hidden = true;
                        return;
                    }

                    activityIndicator.View.RemoveFromSuperview();
                });
        }

        public static ActivityIndicator Start(UIView view)
        {
            var widget = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);

            widget.Frame = new RectangleF (
                (view.Frame.Width / 2) - (widget.Frame.Width / 2),
                (view.Frame.Height / 2) - (widget.Frame.Height / 2),
                widget.Frame.Width,
                widget.Frame.Height);

            widget.Color = UIColor.DarkGray;
            widget.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;
            view.AddSubview(widget);
            widget.StartAnimating();

            return new ActivityIndicator(widget, true);
        }

        public static ActivityIndicator Start(UIActivityIndicatorView activityIndicator)
        {
            return Start(activityIndicator, true);
        }

        public static ActivityIndicator Start(UIActivityIndicatorView activityIndicator, bool fadeIn)
        {
            activityIndicator.StartAnimating();

            if (fadeIn)
            {
                activityIndicator.Alpha = 0;
                activityIndicator.Hidden = false;
                UIView.Animate(0.3, () => { activityIndicator.Alpha = 1; });
            }
            else
            {
                activityIndicator.Hidden = false;
            }

            return new ActivityIndicator(activityIndicator, false);
        }

        private readonly UIActivityIndicatorView _view;
        private readonly bool _isOwner;

        private ActivityIndicator(UIActivityIndicatorView view, bool isOwner)
        {
            _view = view;
            _isOwner = isOwner;
        }

        public void Dispose()
        {
            Stop(this);
        }

        private UIActivityIndicatorView View
        {
            get { return _view; }
        }

        public bool IsOwner
        {
            get { return _isOwner; }
        }
    }
}

