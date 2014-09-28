using System;
using MonoTouch.UIKit;

namespace softWrench.iOS.Views
{
    /// <summary>
    ///     Implements <seealso cref="IUITextInput"/>
    ///     for an iOS <seealso cref="UITextField"/>.
    /// </summary>
	public partial class DetailTextField : UITextField, IUITextInput
	{
        public DetailTextField (IntPtr handle) : base (handle)
		{
		}
	}
}
