//

using System;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using softWrench.Mobile.Utilities;

namespace softWrench.iOS.Utilities {
    /// <summary>
    /// Theme class for appling UIAppearance to the app, and holding app level resources (colors and images)
    /// * Notice use of Lazy&lt;T&gt;, so images are not loaded until requested
    /// </summary>
    internal static class Theme {
        #region Images

        private static readonly Lazy<UIImage> ListIconF = new Lazy<UIImage>(() => UIImage.FromFile("Images/listicon.png"));

        /// <summary>
        /// Assignment list icon for tab bar
        /// </summary>
        public static UIImage ListIcon {
            get { return ListIconF.Value; }
        }

        private static readonly Lazy<UIImage> NumberBoxF = new Lazy<UIImage>(() => UIImage.FromFile("Images/numberbox.png").CreateResizableImage(new UIEdgeInsets(11, 11, 11, 11)));

        /// <summary>
        /// Background image for numbers on assignments
        /// </summary>
        public static UIImage NumberBox {
            get { return NumberBoxF.Value; }
        }

        static readonly Lazy<UIImage> LoginButtonF = new Lazy<UIImage>(() => UIImage.FromFile("Images/login_btn.png").CreateResizableImage(new UIEdgeInsets(11, 8, 11, 8)));

        /// <summary>
        /// Login button on first screen
        /// </summary>
        public static UIImage LoginButton {
            get { return LoginButtonF.Value; }
        }

        static readonly Lazy<UIImage> LoginTextFieldF = new Lazy<UIImage>(() => UIImage.FromFile("Images/login_textfield.png").CreateResizableImage(new UIEdgeInsets(10, 10, 10, 10)));

        /// <summary>
        /// Login text field on first screen
        /// </summary>
        public static UIImage LoginTextField {
            get { return LoginTextFieldF.Value; }
        }

        #endregion

        #region Colors

        private static readonly Lazy<UIColor> LabelColorF = new Lazy<UIColor>(() => UIColor.FromRGB(0x33, 0x33, 0x33));

        /// <summary>
        /// General label color for the entire app
        /// </summary>
        public static UIColor LabelColor {
            get { return LabelColorF.Value; }
        }

        #endregion

        /// <summary>
        /// Apply UIAppearance to this application, this is iOS's version of "styling"
        /// </summary>
        public static void Apply(UIWindow window) {
            window.TintColor = UIColor.FromRGB(0, 173, 239);

            UINavigationBar.Appearance.BarTintColor = UIColor.FromRGB(0, 173, 239);
            UIBarButtonItem.Appearance.TintColor = UIColor.White;

            //UIActivityIndicatorView.Appearance.Color = IndicatorColor;
            //UIToolbar.Appearance.SetBackgroundImage (BlueBar, UIToolbarPosition.Any, UIBarMetrics.Default);
            //UIBarButtonItem.Appearance.SetBackButtonBackgroundImage (BackButton, UIControlState.Normal, UIBarMetrics.Default);
        }

        private const string FontName = "HelveticaNeue-Medium";
        private const string BoldFontName = "HelveticaNeue-Bold";
        private const string LightFontName = "HelveticaNeue-Light";

        /// <summary>
        /// Returns the default font with a certain size
        /// </summary>
        public static UIFont FontOfSize(float size) {
            return UIFont.FromName(FontName, size);
        }

        /// <summary>
        /// Returns the default font with a certain size
        /// </summary>
        public static UIFont BoldFontOfSize(float size) {
            return UIFont.FromName(BoldFontName, size);
        }

        /// <summary>
        /// Returns the default font with a certain size
        /// </summary>
        public static UIFont LightFontOfSize(float size) {
            return UIFont.FromName(LightFontName, size);
        }

        /// <summary>
        /// Transitions a controller to the rootViewController, for a fullscreen transition
        /// </summary>
        public static void TransitionController(UIViewController controller, bool animated = true) {
            var window = ServiceContainer.Resolve<UIWindow>();

            if (window.RootViewController == controller)
                return;

            window.RootViewController = controller;

            if (animated)
                UIView.Transition(window, .3, UIViewAnimationOptions.TransitionCrossDissolve, delegate { }, delegate { });
        }

        public static readonly CGColor UltraLightColor = new CGColor(0.85f, 0.85f, 0.85f);
        public static readonly UIColor DarkBlueColor = UIColor.FromRGB(0, 86, 120);
        public static readonly UIColor DarkerBlueColor = UIColor.FromRGB(0, 44, 61);
        public static readonly UIColor DarkestBlueColor = UIColor.FromRGB(0, 12, 32);
        public static readonly UIColor LightBlueColor = UIColor.FromRGB(0, 173, 239);
        public static readonly UIColor LightTextColor = UIColor.FromWhiteAlpha(0.45f, 1f);
        public static readonly CGColor BorderColor = new CGColor(0.8f, 0.8f, 0.8f);
        public static readonly UIColor LightBackgroundColor = UIColor.FromRGB(245, 245, 245);

        public static readonly Func<UIImage> SplashLandscape = () => UIImage.FromBundle("Default-Landscape.png");
        public static readonly Func<UIImage> SplashPortrait = () => UIImage.FromBundle("Default-Portrait.png");

        private static readonly Lazy<UIImage> LogoF = new Lazy<UIImage>(() => UIImage.FromFile("Images/softwrench-logo.png"));
        private static readonly Func<UIImage> DetailEmptyStateImageF = () => UIImage.FromFile("Images/detail-empty-state.png");
        private static readonly Lazy<UIImage> ErrorIconF = new Lazy<UIImage>(() => UIImage.FromFile("Images/error.png"));
        private static readonly UIColor TextColorF = UIColor.FromWhiteAlpha(0.1f, 1);
        private static readonly UIColor ErrorColorF = UIColor.FromRGB(165, 48, 30);
        private static readonly UIColor NonReadOnlyTextColorF = UIColor.FromWhiteAlpha(0.1f, 1);
        private static readonly UIColor ReadOnlyTextColorF = UIColor.FromWhiteAlpha(0.5f, 1);

        // ReSharper disable once InconsistentNaming
        public static CGColor AsCGColor(this UIColor color) {
            float r, g, b, a;
            color.GetRGBA(out r, out g, out b, out a);

            return new CGColor(r, g, b, a);
        }

        public static UIImage Logo {
            get { return LogoF.Value; }
        }

        public static UIImage ErrorIcon {
            get { return ErrorIconF.Value; }
        }

        private static readonly Lazy<UIImage> CommandButtonLazy = new Lazy<UIImage>(() => UIImage.FromFile("Images/login_btn.png").CreateResizableImage(new UIEdgeInsets(11, 8, 11, 8)));

        private static readonly Lazy<UIImage> NavButtonLazy = new Lazy<UIImage>(() => UIImage.FromFile("Images/login_btn.png").CreateResizableImage(new UIEdgeInsets(20, 8, 20, 8)));

        public static UIImage CommandButton {
            get { return CommandButtonLazy.Value; }
        }

        public static UIImage NavButton {
            get { return NavButtonLazy.Value; }
        }

        public static UIColor NonReadOnlyTextColor {
            get { return NonReadOnlyTextColorF; }
        }

        public static UIColor ReadOnlyTextColor {
            get { return ReadOnlyTextColorF; }
        }

        public static UIColor TextColor {
            get { return TextColorF; }
        }

        public static UIColor ErrorColor {
            get { return ErrorColorF; }
        }

        public static UIImage DetailEmptyStateImage {
            get { return DetailEmptyStateImageF(); }
        }

        public static UIImage SaveBackground() {
            return UIImage.FromFile("Images/dark-blue-background.png").CreateResizableImage(new UIEdgeInsets(11, 11, 11, 11));
        }

        public static UIImage MenuIcon() {
            return UIImage.FromFile("Images/menu.png");
        }

        public static UIImage SaveIcon() {
            return UIImage.FromFile("Images/save.png").ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
        }


        public static UIImage HomeIcon() {
            return UIImage.FromFile("Images/home.png").ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
        }

        public static UIImage ClipboardIcon() {
            return UIImage.FromFile("Images/clipboard.png").ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
        }

        public static void BorderHorizontally(UITableViewCell cell, NSIndexPath indexPath) {
            //            var background = new UIView();
            //
            //            if (indexPath.Row == 0)
            //            {
            //                var topBorder = new CALayer();
            //                topBorder.Frame = new RectangleF(44, 0, 703, 1f);
            //                topBorder.BackgroundColor = Theme.UltraLightColor;
            //                background.Layer.AddSublayer(topBorder);
            //            }
            //
            //            var bottomBorder = new CALayer();
            //            bottomBorder.Frame = new RectangleF(44, cell.Frame.Height - 1, 703, 1f);
            //            bottomBorder.BackgroundColor = Theme.UltraLightColor;
            //            background.Layer.AddSublayer(bottomBorder);
            //
            //            background.ClipsToBounds = true;
            //            cell.BackgroundView = background;
        }

        public static void BorderNone(UITableViewCell cell) {
            //            var background = new UIView();
            //             
            //            background.ClipsToBounds = true;
            //            cell.BackgroundView = background;
        }

        public static void Fade(CALayer layer) {
            var transition = new CATransition {
                Duration = 0.250,
                Type = "kCATransitionFade",
                Subtype = "kCATransitionFromTop"
            };

            layer.AddAnimation(transition, "kCATransition");
        }

        public static void Fade(UIView view) {
            Fade(view.Layer);
        }
    }
}

