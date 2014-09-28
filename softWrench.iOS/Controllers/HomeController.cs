using System;
using System.Drawing;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using softWrench.iOS.UI.Eventing;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Communication;
using softWrench.Mobile.Communication.Synchronization;
using softWrench.Mobile.Exceptions;

namespace softWrench.iOS.Controllers {
    public partial class HomeController : BaseController {
        private static void ShowSynchronizationResultMessage(SynchronizationResult result) {
            var message = 0 == result.Errors
                ? "Data synchronized with success."
                : "Part of your work could not be synchronized to the server and is still pending.";

            Alert.Show("Data Synchronized", message);
        }

        public HomeController(IntPtr handle)
            : base(handle) {
        }

        private async Task SynchronizeAsyncImpl() {
            SynchronizationResult result;

            using (ActivityIndicator.Start(View)) {
                result = await new SynchronizationFacade().Synchronize();
                SimpleEventBus.Publish(new DataSynchronized());
            }

            ShowSynchronizationResultMessage(result);
        }

        private void ReSignIn() {
            // Displays a modal authentication form so
            // the user can re-validate its cookie.
            var reSignInController = Storyboard.InstantiateViewController<ReSignInController>();
            reSignInController.Construct(OnReSignIn);

            PresentViewController(reSignInController, true, null);

            // It's important to resize the view *after*
            // the animation and outside ViewDidLoad or
            // WillAppear. Otherwise the view blinks.
            reSignInController
                .View
                .Superview
                .Bounds = new RectangleF(0, 0, 420, 360);
        }

        private async void OnReSignIn(ReSignInController.Result result) {
            // If the user canceled the
            // sign in we can't do much.
            if (false == result.IsSuccess) {
                return;
            }

            await SynchronizeAsync();
        }

        private async Task SynchronizeAsync() {
            try {
                await SynchronizeAsyncImpl();
            } catch (HttpUnauthorizedRequestException) {
                // Oops, our authentication cookie
                // must have expired. Let's ask for
                // credentials again.
                ReSignIn();
            }
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        async partial void Synchronize(NSObject sender) {
            if (false == NetworkStatus.IsReachable()) {
                Alert.Show("No Connectivity", "Our server seems to be unreachable. Did you check your internet access?");
                return;
            }

            try {
                await SynchronizeAsync();
            } catch (Exception e) {
                //TODO: show user-friendly message.
                Alert.Show("Synchronization Failed", e.ToString());
            }
        }

        public override void ViewDidLoad() {
            base.ViewDidLoad();

            synchronizeButton.SetBackgroundImage(Theme.CommandButton, UIControlState.Normal);
        }
    }
}