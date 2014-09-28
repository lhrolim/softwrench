using softwrench.sW4.Shared2.Metadata.Applications.UI;

namespace softWrench.Mobile.Metadata.Applications.UI
{
    public interface IWidget :IWidgetDefinition
    {
        /// <summary>
        ///     Formats the specified raw value according
        ///     to the widget specification.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns></returns>
        string Format(string value);

        /// <summary>
        ///     Evaluates whether the provided value satisfies the
        ///     validation conditions specified by the widget.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="error">
        ///     Returns an user-friendly error message if it does
        ///     not satisfies the conditions, otherwise returns
        ///     <see langword="null"/>.
        /// </param>
        bool Validate(string value, out string error);
    }
}

