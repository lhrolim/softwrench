using System.ComponentModel;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using softWrench.iOS.Applications;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.Persistence;
using softWrench.Mobile.Utilities;

namespace softWrench.iOS
{
	/// <summary>
	/// AppDelegate, the main callback for application-level events in iOS
	/// </summary>    
	[Register ("AppDelegate")]
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class AppDelegate : UIApplicationDelegate
	{
		private UIWindow _window;
		private UIStoryboard _storyboard;

		/// <summary>
		/// This the main entry point for the app on iOS
		/// </summary>
		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			//Create our window
			_window = new UIWindow(UIScreen.MainScreen.Bounds);

			//Register some services
			ServiceContainer.Register (_window);
			ServiceContainer.Register <ISynchronizeInvoke>(() => new SynchronizeInvoke());
		    ApplicationBehaviorDispatcher.PlatformSpecificProbingNamespace = typeof (NamespaceAnchor).Namespace;

			//Apply our UI theme
			Theme.Apply (_window);

			//Load our storyboard and setup our UIWindow and first view controller
			_storyboard = UIStoryboard.FromName ("MainStoryboard", null);
			_window.RootViewController = (UIViewController) _storyboard.InstantiateInitialViewController();
			_window.MakeKeyAndVisible ();

		    Database.Initialize();

			return true;
		}

		/// <summary>
		/// This is how orientation is setup on iOS 6
		/// </summary>
		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations (UIApplication application, UIWindow forWindow)
		{
			return UIInterfaceOrientationMask.All;
		}

		/// <summary>
		/// Event when the app is backgrounded, or screen turned off
		/// </summary>
		public override void DidEnterBackground (UIApplication application)
		{
		}

		/// <summary>
		/// Event when the app is brought back to the foreground or screen unlocked
		/// </summary>
		public override void WillEnterForeground (UIApplication application)
		{			
		}
	}
}

