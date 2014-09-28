using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Communication.SignIn;
using softWrench.Mobile.Exceptions;
using softWrench.Mobile.Utilities;

namespace softWrench.iOS.Controllers {
    public partial class SignInController : BaseController {
        private static void Label(UITextField textField, string label) {
            textField.LeftViewMode = UITextFieldViewMode.Always;
            textField.LeftView = new UILabel(new RectangleF(0, 3, 70, 24)) {
                Font = Theme.LightFontOfSize(12),
                Text = label,
                TextAlignment = UITextAlignment.Right,
                TextColor = Theme.LightTextColor
            };
        }

        private readonly Animator _animator;
        private bool _wasLandscape = true;

        public SignInController(IntPtr handle)
            : base(handle) {
            _animator = new Animator(this);
        }

        private void WarnAbout(UIView label) {
            label.Alpha = 0;
            label.Hidden = false;
            signInButton.UserInteractionEnabled = true;

            UIView.Animate(0.3, () => {
                label.Alpha = 1;
                signInButton.Alpha = 1;
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

        private void NavigateToHome() {
            var tabController = Storyboard.InstantiateViewController<TabController>();
            Theme.TransitionController(tabController);
        }

        private void SwitchOrientation(UIInterfaceOrientation orientation, bool animated, double duration = 0.5) {
            if (orientation.IsLandscape() && false == _wasLandscape) {
                _animator.Rotate(orientation, animated, duration);
                _wasLandscape = true;
            } else if (orientation.IsPortrait() && _wasLandscape) {
                _animator.Rotate(orientation, animated, duration);
                _wasLandscape = false;
            }
        }

        private void ShowSignInForm() {
            _animator.Show(() => username.BecomeFirstResponder());
        }

        private void HideSignInForm() {
            container.Hidden = true;
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
                    NavigateToHome();
                    break;
                case RemoteSignInResult.Unreachable:
                    WarnAboutNoConnectivity();
                    break;
                case RemoteSignInResult.BadCredentials:
                    WarnAboutBadCredentials();
                    break;
            }
        }

        private async void SignInLocallyAsync() {
            LocalSignInResult result;

            using (ActivityIndicator.Start(indicator)) {
                try {
                    result = await ServiceContainer
                        .Resolve<ISignIn>()
                        .SignInLocallyAsync();
                } catch (InvalidSettingsException) {
                    // If he have no settings, we'll consider
                    // the login as expired so the user has a
                    // chance to change them in a new sign in.
                    result = LocalSignInResult.Expired;
                }
            }

            switch (result) {
                case LocalSignInResult.Expired:
                    ShowSignInForm();
                    return;
                case LocalSignInResult.Success:
                    NavigateToHome();
                    break;
            }
        }

        private void OnSettingsCompleted(bool isSuccess) {
            username.BecomeFirstResponder();
        }

        // ReSharper disable once UnusedParameter.Local
        async partial void SignIn(UIButton sender) {
            noConnectivity.Hidden = true;
            badCredentials.Hidden = true;
            invalidSettings.Hidden = true;
            username.ResignFirstResponder();
            password.ResignFirstResponder();

            // Hides the sign in button while
            // the operation is carried on.
            signInButton.UserInteractionEnabled = false;
            UIView.Animate(0.3, () => { signInButton.Alpha = 0.2f; }, null);

            await SignInRemotelyAsync();
        }

        // ReSharper disable once UnusedParameter.Local
        partial void Settings(UIButton sender) {
            var controller = Storyboard.InstantiateViewController<SettingsController>();
            controller.Construct(OnSettingsCompleted);
            PresentViewController(controller, true, null);
        }

        protected override void OnKeyboardChanged(bool visible, float height) {
            _animator.Nudge(visible);
        }

        public override void ViewDidLoad() {
            base.ViewDidLoad();

            signInButton.SetBackgroundImage(Theme.LoginButton, UIControlState.Normal);

            logo.Image = Theme.Logo;
            indicator.Hidden = true;

            Label(username, "Username");
            username.TextColor = Theme.LabelColor;
            username.Background = Theme.LoginTextField;
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

            _wasLandscape = InterfaceOrientation.IsPortrait();

            HideSignInForm();
            SwitchOrientation(InterfaceOrientation, false);
        }

        public override void ViewWillAppear(bool animated) {
            base.ViewWillAppear(animated);

            // We'll try our luck searching for
            // a locally authenticated user.
            SignInLocallyAsync();
        }

        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration) {
            base.WillRotate(toInterfaceOrientation, duration);
            SwitchOrientation(toInterfaceOrientation, true, duration);
        }

        public override bool HandlesKeyboardNotifications {
            get {
                // We're interested on hearing
                // about the OnKeyboardChanged
                // provided by our base.
                return true;
            }
        }

        private class Animator {
            private const string TopBorderName = "topBorder";
            private const string BottomBorderName = "bottomBorder";
            private const string LeftBorderName = "leftBorder";
            private const int NudgeOffset = 120;

            private static void RemoveBorder(UIView view, string name) {
                var border = view
                    .Layer
                    .Sublayers
                    .FirstOrDefault(l => l.Name == name);

                if (null != border) {
                    border.RemoveFromSuperLayer();
                }
            }

            private static void AddBorder(UIView view, RectangleF frame, string name) {
                RemoveBorder(view, name);

                var border = new CALayer {
                    Frame = frame,
                    Name = name,
                    BackgroundColor = Theme.UltraLightColor
                };

                view.Layer.AddSublayer(border);
            }

            private static void BorderLeft(UIView view) {
                RemoveBorder(view, TopBorderName);
                RemoveBorder(view, BottomBorderName);

                var leftBorder = new RectangleF(0, 0, 1f, view.Frame.Height);
                AddBorder(view, leftBorder, LeftBorderName);
            }

            private static void BorderHorizontally(UIView view) {
                RemoveBorder(view, LeftBorderName);

                AddBorder(view, new RectangleF(-100, 0, view.Frame.Width + 200, 1f), TopBorderName);
                AddBorder(view, new RectangleF(-100, view.Frame.Height, view.Frame.Width + 200, 1f), BottomBorderName);
            }

            private static void Translate(UIView view, int x, int y, bool animate = false) {
                var translate = new NSAction(() => {
                                                 var copy = view.Frame;
                                                 copy.X = x;
                                                 copy.Y = y;

                                                 view.Frame = copy;
                                             });

                if (false == animate) {
                    translate();
                    return;
                }

                UIView.Animate(.3, 0, UIViewAnimationOptions.CurveEaseInOut, translate, null);
            }

            private readonly SignInController _controller;
            private bool _isNudged;

            public Animator(SignInController controller) {
                _controller = controller;
            }

            private void TranslateAndShrinkLogo() {
                var frame = _controller.logo.Frame;

                // We need to move and resize the logo
                // using the same frame, otherwise the
                // animation suffers a little hiccup.
                if (_controller.InterfaceOrientation.IsLandscape()) {
                    frame.X = 110;
                    frame.Y = 300;
                } else {
                    frame.X = 192;
                    frame.Y = 252;
                }

                frame.Width = 384;
                frame.Height = 175;

                _controller.logo.Frame = frame;
            }

            private void TranslateAndExpandLogo() {
                var frame = _controller.logo.Frame;

                // We need to move and resize the logo
                // using the same frame, otherwise the
                // animation suffers a little hiccup.
                if (_controller.InterfaceOrientation.IsLandscape()) {
                    frame.X = 272f;
                    frame.Y = 292.2f;
                } else {
                    frame.X = 142.2f;
                    frame.Y = 423.2f;
                }

                frame.Width = 479;
                frame.Height = 202;

                _controller.logo.Frame = frame;
            }

            private void TranslateLogo(UIInterfaceOrientation orientation, bool animate) {
                var offset = _isNudged ? -NudgeOffset : 0;

                if (orientation.IsLandscape()) {
                    Translate(_controller.logo, 110, 300 + offset, animate);
                } else {
                    Translate(_controller.logo, 192, 252 + offset, animate);
                }
            }

            private void TranslateLogo() {
                TranslateLogo(_controller.InterfaceOrientation, true);
            }

            private void TranslateSignInForm(UIInterfaceOrientation orientation) {
                var offset = _isNudged ? -NudgeOffset : 0;

                if (orientation.IsLandscape()) {
                    Translate(_controller.container, 556, 240 + offset);
                    BorderLeft(_controller.container);
                } else {
                    Translate(_controller.container, 222, 428 + offset);
                    BorderHorizontally(_controller.container);
                }
            }

            private void TranslateSignInForm() {
                TranslateSignInForm(_controller.InterfaceOrientation);
            }

            private void RotateSplashMode(UIInterfaceOrientation orientation) {
                var isPortrait = orientation.IsPortrait();

                var image = isPortrait
                    ? Theme.SplashPortrait()
                    : Theme.SplashLandscape();

                var splash = _controller.splash;

                // Some magic numbers to hold the background
                // in place and provide a smooth transition
                // from the launch image. TODO: test @retina,
                // also seems to cause glitches on iOS7.
                splash.Frame = new RectangleF(splash.Frame.Left, 0, splash.Frame.Width * (1024f / 1004), 1024);
                splash.Image = image;


                // Align the logo (hidden behind the splash)
                // to the logo that is part of the splash,
                // thus creating a smooth transition when
                // the splash disappears.
                TranslateAndExpandLogo();
            }

            private void RotateSignInMode(UIInterfaceOrientation orientation) {
                // If we're in portrait orientation, all
                // controls are visible on the UI and no
                // nudging is necessary.
                if (orientation.IsPortrait()) {
                    _isNudged = false;
                }

                // We'll move the logo but not animate it,
                // as we're taking care of the animation
                // ourselves.
                TranslateLogo(orientation, false);
                TranslateSignInForm(orientation);
            }

            public void Rotate(UIInterfaceOrientation orientation, bool animated, double duration) {
                if (animated) {
                    UIView.BeginAnimations("SwitchOrientation");
                    UIView.SetAnimationDuration(duration);
                    UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);
                }

                // We'll rotate different content
                // based on the current state of
                // the controller.
                if (IsSplashMode) {
                    RotateSplashMode(orientation);
                } else {
                    RotateSignInMode(orientation);
                }

                if (animated) {
                    UIView.CommitAnimations();
                }
            }

            public void Nudge(bool isKeyboardVisible) {
                // If we're in portrait orientation, all
                // controls are visible on the UI and no
                // nudging is necessary.
                if (_controller.InterfaceOrientation.IsPortrait()) {
                    _isNudged = false;
                    return;
                }

                if (isKeyboardVisible) {
                    _isNudged = true;

                    // The keyboard is now open and we're
                    // landscape. Let's nudge the controls
                    // a little toward the top.
                    TranslateLogo();
                    TranslateSignInForm();
                } else if (_controller._wasLandscape) {
                    _isNudged = false;

                    // The keyboard is now closed. The additional
                    // check on _wasLandscape is to avoid a glitch
                    // if the device is rotated to portrait while
                    // the keyboard is open.
                    TranslateLogo();
                    TranslateSignInForm();
                }
            }

            public void Show(NSAction onCompletion) {
                _controller.logo.Hidden = false;
                _controller.container.Alpha = 0;
                _controller.container.Hidden = false;

                // The sign in form will appear
                // already in its final position.
                TranslateSignInForm();

                UIView.Animate(.5, 0, UIViewAnimationOptions.CurveEaseInOut, () => {
                                                                                 // Shrinks the logo to the left.
                                                                                 TranslateAndShrinkLogo();

                                                                                 _controller.container.Alpha = 1;

                                                                                 // Get rid of the splash image.
                                                                                 _controller.splash.Alpha = 0;
                                                                                 _controller.splash.Hidden = true;

                                                                             }, onCompletion);
            }

            private bool IsSplashMode {
                get { return _controller.container.Hidden; }
            }
        }
    }
}

