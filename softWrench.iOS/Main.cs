using MonoTouch.UIKit;
using softWrench.iOS.Communication;
using softWrench.Mobile.Communication;
using softWrench.Mobile.Communication.SignIn;
using softWrench.Mobile.Utilities;

namespace softWrench.iOS
{
	public class Application
	{
		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		static void Main (string[] args)
		{
            //Services specific for iOS.
            ServiceContainer.Register<ISignIn>((() => new SignIn()));

			// if you want to use a different Application Delegate class from "AppDelegate" you can specify it here.
			UIApplication.Main (args, null, "AppDelegate");
		}
	}
}
