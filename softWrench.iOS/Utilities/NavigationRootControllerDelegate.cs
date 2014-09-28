using System;
using MonoTouch.UIKit;
using softWrench.iOS.UI.Eventing;

namespace softWrench.iOS.Utilities {
    public class NavigationRootControllerDelegate : IDisposable {
        private readonly UIViewController _controller;
        private UIBarButtonItem _showMenuButton;
        private bool _wasLandscape = true;


        public NavigationRootControllerDelegate(UIViewController controller) {
            _controller = controller;
        }

        private void SubscribeToBus() {
            SimpleEventBus.Subscribe<PopoverMenuToggled>(OnPopoverMenuToggled);
        }

        private void UnsubscribeFromBus() {
            SimpleEventBus.Unsubscribe<PopoverMenuToggled>(OnPopoverMenuToggled);
        }

        private void OnTogglePopoverMenu() {
            SimpleEventBus.Publish(new PopoverMenuToggleRequested(true));
            _controller.NavigationItem.SetLeftBarButtonItems(new UIBarButtonItem[0], true);
        }

        private void OnPopoverMenuToggled(PopoverMenuToggled e) {
            if (e.Show) {
                return;
            }

            _controller.NavigationItem.SetLeftBarButtonItems(new UIBarButtonItem[] { _showMenuButton }, true);
        }

        private void SwitchOrientation(UIInterfaceOrientation orientation, bool animated, double duration = .5) {
            if (orientation.IsLandscape()) {
                if (!_wasLandscape) {
                    _controller.NavigationItem.SetLeftBarButtonItems(new UIBarButtonItem[0], true);
                    _wasLandscape = true;
                }
            } else {
                if (_wasLandscape) {
                    _controller.NavigationItem.SetLeftBarButtonItems(new UIBarButtonItem[] { _showMenuButton }, true);
                    _wasLandscape = false;
                }
            }
        }

        public void Dispose() {
            UnsubscribeFromBus();
        }

        public void ViewDidLoad() {
            _controller.NavigationItem.LeftItemsSupplementBackButton = true;
            _showMenuButton = new UIBarButtonItem(Theme.MenuIcon(), UIBarButtonItemStyle.Plain, (sender, e) => OnTogglePopoverMenu()) {
                ImageInsets = new UIEdgeInsets(2, 0, -2, 0)
            };

            SwitchOrientation(_controller.InterfaceOrientation, false);
        }

        public void ViewWillAppear(bool animated) {
            SubscribeToBus();
        }

        public void ViewWillDisappear(bool animated) {
            UnsubscribeFromBus();
        }

        public void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration) {
            //Start an animation to switch orientations
            SwitchOrientation(toInterfaceOrientation, true, duration);
        }
    }
}
