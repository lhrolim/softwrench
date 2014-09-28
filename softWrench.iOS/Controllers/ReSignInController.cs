using System;
using System.Drawing;
using System.Threading.Tasks;
using MonoTouch.UIKit;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Communication.SignIn;
using softWrench.Mobile.Exceptions;
using softWrench.Mobile.Utilities;

namespace softWrench.iOS.Controllers {
    public partial class ReSignInController : BaseController {
        private static void Label(UITextField textField, string label) {
            textField.LeftViewMode = UITextFieldViewMode.Always;
            textField.LeftView = new UILabel(new RectangleF(0, 3, 70, 24)) {
                Font = Theme.LightFontOfSize(12),
                Text = label,
                TextAlignment = UITextAlignment.Right,
                TextColor = Theme.LightTextColor
            };
        }

        private Action<Result> _onCompletion;

        public ReSignInController(IntPtr handle)
            : base(handle) {
        }

        public void Construct(Action<Result> onCompletion) {
            if (onCompletion == null) throw new ArgumentNullException("onCompletion");

            _onCompletion = onCompletion;
        }

        private void WarnAbout(UIView label) {
            label.Alpha = 0;
            label.Hidden = false;
            signInButton.UserInteractionEnabled = true;

            UIView.Animate(0.3, () => {
                label.Alpha = 1;
                signInButton.Alpha = 1;
                sessionExpired.Alpha = 0;
                sessionExpired.Hidden = false;
            });
        }

        private void WarnAboutBadCredentials() {
            WarnAbout(badCredentials);
        }

        private void WarnAboutNoConnectivity() {
            WarnAbout(noConnectivity);
        }

        private void WarnAboutInvalidSettings() {
            WarnAbout(invalidSettings);
        }

        private void OnCompletion(Result result) {
            // Before popping the navigation stack,
            // let's store our completion handler
            // to avoid losing it by cleanup methods. 
            var onCompletion = _onCompletion;

            DismissViewController(true, null);

            if (null == onCompletion) {
                return;
            }

            onCompletion(result);

            // We don't want to keep unnecessary references
            // to (possibly) other controllers.
            _onCompletion = null;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private async Task SignInRemotelyAsync() {
            RemoteSignInResult result;

            using (ActivityIndicator.Start(indicator)) {
                try {
                    result = await ServiceContainer
                        .Resolve<ISignIn>()
                        .SignInRemotelyAsync(username.Text, password.Text);
                } catch (InvalidSettingsException) {
                    WarnAboutInvalidSettings();
                    return;
                }
            }

            switch (result) {
                case RemoteSignInResult.Success:
                    OnCompletion(new Result(true));
                    break;
                case RemoteSignInResult.Unreachable:
                    WarnAboutNoConnectivity();
                    break;
                case RemoteSignInResult.BadCredentials:
                    WarnAboutBadCredentials();
                    break;
            }
        }

        // ReSharper disable once UnusedParameter.Local
        async partial void SignIn(UIButton sender) {
            username.ResignFirstResponder();
            password.ResignFirstResponder();

            // Hides the sign in button while
            // the operation is carried on.
            signInButton.UserInteractionEnabled = false;
            UIView.Animate(0.3, () => { signInButton.Alpha = 0.2f; }, null);

            await SignInRemotelyAsync();
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        partial void Cancel(UIButton sender) {
            OnCompletion(new Result(false));
        }

        public override void ViewDidLoad() {
            base.ViewDidLoad();

            signInButton.SetBackgroundImage(Theme.LoginButton, UIControlState.Normal);

            indicator.Hidden = true;

            Label(username, "Username");
            username.TextColor = Theme.LabelColor;
            username.Background = Theme.LoginTextField;
            username.BecomeFirstResponder();
            username.ShouldReturn = sender => {
                                        password.BecomeFirstResponder();
                                        return true;
                                    };

            Label(password, "Password");
            password.TextColor = Theme.LabelColor;
            password.Background = Theme.LoginTextField;
            password.ShouldReturn = sender => {
                                        password.ResignFirstResponder();
                                        SignIn(signInButton);
                                        return true;
                                    };
        }

        public class Result {
            private readonly bool _isSuccess;

            public Result(bool isSuccess) {
                _isSuccess = isSuccess;
            }

            public bool IsSuccess {
                get { return _isSuccess; }
            }
        }
    }
}