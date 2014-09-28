using System;
using MonoTouch.UIKit;

namespace softWrench.iOS.Views
{
    /// <summary>
    ///     Shares a common contract abstracting
    ///     either an <seealso cref="UITextField"/>
    ///     or an <see cref="UITextView"/>.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public interface IUITextInput
    {
        /// <summary>
        ///     Notifies the control's receiver that it has
        ///     been asked to relinquish its status as first
        ///     responder in its window.
        /// </summary>
        bool ResignFirstResponder();

        /// <summary>
        ///     Gets or sets the type of keyboard
        ///     to use with the view.
        /// </summary>
        UIKeyboardType KeyboardType { get; set; }

        /// <summary>
        ///     Delegate invoked by the object to get a value.
        /// </summary>
        UITextFieldCondition ShouldReturn { get; set; }

        /// <summary>
        ///     Gets or sets the text to display.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        ///     Gets or sets the color of the text.
        /// </summary>
        UIColor TextColor { get; set; }

        /// <summary>
        ///     Determines whether input events
        ///     are processed by this view.
        /// </summary>
        bool UserInteractionEnabled { get; set; }
    }
}