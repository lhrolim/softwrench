using System;
using MonoTouch.UIKit;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Persistence;

namespace softWrench.iOS.Controllers
{
	public partial class SettingsController : BaseController
	{
        private readonly Settings _settings;
        private Action<bool> _onCompletion;
        private bool _completionIsSuccess;

		public SettingsController (IntPtr handle) : base (handle)
		{
            _settings = User.Settings ?? new Settings();
		}

        public void Construct(Action<bool> onCompletion)
        {
            _onCompletion = onCompletion;
            _completionIsSuccess = false;
        }

        private bool TryParseServer(out string value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(server.Text))
            {
                Alert.Show("Oops...", "Please provide the server address.");
                server.BecomeFirstResponder();
                return false;
            }

            var parsed = server.Text.Trim();
            if (false == parsed.StartsWith("http"))
            {
                parsed = "http://" + parsed;
            }

            Uri uri;
            if (false == Uri.TryCreate(parsed, UriKind.Absolute, out uri))
            {
                Alert.Show("Oops...", "The server address does not seem to be valid.");
                server.BecomeFirstResponder();
                return false;
            }

            value = uri.ToString();
            return true;
        }

        private void OnCompletion()
        {
            // Before popping the navigation stack,
            // let's store our completion handler
            // to avoid losing it by cleanup methods. 
            var onCompletion = _onCompletion;

            if (null == onCompletion)
            {
                return;
            }

            onCompletion(_completionIsSuccess);

            // We don't want to keep unnecessary references
            // to (possibly) other controllers.
            _onCompletion = null;
        }

        protected override void Dispose(bool disposing)
        {
            _onCompletion = null;
            base.Dispose(disposing);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            saveButton.SetBackgroundImage(Theme.LoginButton, UIControlState.Normal);
            server.Text = _settings.Server;
            server.ShouldReturn = sender =>
            {
                server.ResignFirstResponder();
                return true;
            };
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            OnCompletion();
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        async partial void Save(UIBarButtonItem sender)
        {
            string parsedServer;

            if (false == TryParseServer(out parsedServer))
            {
                return;
            }

            _settings.Server = parsedServer;

            await new UserRepository()
                .SaveAndApplySettingsAsync(_settings);
                        
            _completionIsSuccess = true;
            DismissViewController(true, null);
        }
	}
}
