using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using softWrench.iOS.Mobile.Metadata.Extensions;
using softWrench.iOS.UI.Eventing;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Metadata.Extensions;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Menu;

namespace softWrench.iOS.Controllers {
    public partial class TabController : UITabBarController {
        private UIActionSheet _actionSheet;
        private static readonly MetadataRepository MetadataRepository = MetadataRepository.GetInstance();

        public TabController(IntPtr handle)
            : base(handle) {
            //Title = "Werbemittel";

            //Hook up a "fade" animation between tabs
            ShouldSelectViewController = (tabController, controller) => {
                if (SelectedViewController == null || controller == SelectedViewController)
                    return true;

                var fromView = SelectedViewController.View;
                var toView = controller.View;

                UIView.Transition(fromView, toView, .3f, UIViewAnimationOptions.TransitionCrossDissolve, null);
                return true;
            };
        }

        private void SubscribeToBus() {
            SimpleEventBus.Subscribe<DataSynchronized>(OnDataSynchronized);
        }

        private void UnsubscribeFromBus() {
            SimpleEventBus.Unsubscribe<DataSynchronized>(OnDataSynchronized);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private async Task LoadApplicationsAsync() {
            var interactiveApplications = await GetInteractiveApplications();

            var controllers = new UIViewController[interactiveApplications.Length + 1];

            // The first item on the tab bar
            // will be our home controller.
            var homeController = Storyboard.InstantiateViewController<HomeController>();
            homeController.TabBarItem.Image = Theme.HomeIcon();
            homeController.TabBarItem.SetTitleTextAttributes(new UITextAttributes() { TextColor = UIColor.DarkGray }, UIControlState.Normal);
            var i = 0;
            controllers[i++] = homeController;

            // TODO: optimize this! Each tab will
            // result on data fetch. Lazy load-it
            foreach (var application in interactiveApplications) {
                var controller = Storyboard.InstantiateViewController<ApplicationController>();
                controller.Construct(application);

                var isWorkOrderApplication = "workorder".Equals(application.Name,
                    StringComparison.InvariantCultureIgnoreCase);

                // TODO: make the icon a configuration.
                controller.TabBarItem.Image = isWorkOrderApplication
                    ? Theme.ClipboardIcon()
                    : Theme.ListIcon;
//                controller.TabBarItem.Image.
                controller.TabBarItem.Title = application.Title;
                controller.TabBarItem.SetTitleTextAttributes(new UITextAttributes() { TextColor = UIColor.DarkGray}, UIControlState.Normal);
//                controller.TabBarItem.SetTitleTextAttributes(new UITextAttributes() { TextShadowColor = UIColor.Red }, UIControlState.Normal);

                controllers[i++] = controller;
            }

            ViewControllers = controllers;
        }

        private static async Task<ApplicationSchemaDefinition[]> GetInteractiveApplications() {
            var applications = await MetadataRepository.LoadAllApplicationsAsync();
            var menu = await MetadataRepository.LoadMenuAsync();
            if (applications.Count ==0 || menu == null) {
                //this means first time installation ==> needs to synchronize first
                return new ApplicationSchemaDefinition[] { };
            }

            // We'll create a tab item for each
            // application enabled to the user.
            var interactiveApplications = applications
                .Where(a => menu.IsApplicationOnMenu(a.ApplicationName))
                .ToArray();
            return interactiveApplications;
        }

        private void BorderTabBar() {
            const string layerName = "topBorder";

            var previousBorder = TabBar
                .Layer
                    .Sublayers
                    .FirstOrDefault(l => l.Name == layerName);

            // Ensures the previous border is
            // removed before we add a new one.
            if (null != previousBorder) {
                previousBorder.RemoveFromSuperLayer();
            }

            var topBorder = new CALayer() {
                BackgroundColor = Theme.DarkestBlueColor.AsCGColor(),
                Frame = new RectangleF(0, 0, TabBar.Frame.Width * 2, 1f),
                Name = layerName
            };

            TabBar.Layer.AddSublayer(topBorder);
        }

        private async void OnDataSynchronized(DataSynchronized e) {
            await LoadApplicationsAsync();
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                UnsubscribeFromBus();
            }

            base.Dispose(disposing);
        }

        public async override void ViewDidLoad() {
            base.ViewDidLoad();

            //TabBar.TintColor = UIColor.FromRGB (0x28, 0x2b, 0x30);
            //settings.SetBackgroundImage (Theme.DarkBarButtonItem, UIControlState.Normal, UIBarMetrics.Default);

            SubscribeToBus();
            BorderTabBar();

            await LoadApplicationsAsync();
        }

        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration) {
            base.WillRotate(toInterfaceOrientation, duration);

            BorderTabBar();
        }

        public override void ViewWillDisappear(bool animated) {
            base.ViewWillDisappear(animated);
            UnsubscribeFromBus();
        }

        /// <summary>
        /// This is how orientation is setup on iOS 6
        /// </summary>
        public override bool ShouldAutorotate() {
            return true;
        }

        /// <summary>
        /// This is how orientation is setup on iOS 6
        /// </summary>
        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations() {
            return UIInterfaceOrientationMask.All;
        }

        /// <summary>
        /// Event when the settings toolbar item is clicked
        /// </summary>
        partial void Settings(NSObject sender) {
            if (_actionSheet == null) {
                _actionSheet = new UIActionSheet();
                _actionSheet.AddButton("Logout");
                _actionSheet.Dismissed += (s, e) => {
                    if (e.ButtonIndex == 0) {
                        var signInController = Storyboard.InstantiateViewController<SignInController>();
                        Theme.TransitionController(signInController);
                    }

                    _actionSheet.Dispose();
                    _actionSheet = null;
                };
                _actionSheet.ShowFrom(sender as UIBarButtonItem, true);
            } else {
                _actionSheet.DismissWithClickedButtonIndex(-1, true);
            }
        }
    }
}
