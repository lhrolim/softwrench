using System;
using MonoTouch.UIKit;

namespace softWrench.iOS.Views
{
    /// <summary>
    ///     Implements <seealso cref="IUITextInput"/>
    ///     for an iOS <seealso cref="UITextView"/>.
    /// </summary>
    // ReSharper disable once InconsistentNaming
	public partial class DetailTextView : UITextView, IUITextInput
	{
        public DetailTextView (IntPtr handle) : base (handle)
        {
        }

        /// <summary>
        ///     To avoid breaking iOS HIG, the keyboard is
        ///     not dismissed if Return is pressed, so this
        ///     delegate is actually never invoked.
        /// </summary>
        public UITextFieldCondition ShouldReturn { get; set; }
	}
}
