using System;
using System.ComponentModel;
using softWrench.Mobile.Metadata.Applications.UI;
using softWrench.Mobile.Metadata.Extensions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.UI.Binding
{
    internal class FieldBinding : IDataErrorInfo
    {
        private readonly ApplicationFieldDefinition _metadata;
        private readonly IValueProvider _valueProvider;

        public FieldBinding(ApplicationFieldDefinition metadata, IValueProvider valueProvider)
        {
            if (metadata == null) throw new ArgumentNullException("metadata");
            if (valueProvider == null) throw new ArgumentNullException("valueProvider");

            _metadata = metadata;
            _valueProvider = valueProvider;
        }

        private bool Validate(out string error)
        {
            // If the validation on the form is
            // suppressed, no sweat, let's simply
            // return no errors.
            if (IsValidationSuppressed)
            {
                error = null;
                return true;
            }

            // Ensure required fields are provided.
            if (_metadata.IsRequired && string.IsNullOrWhiteSpace(ValueProvider.Value))
            {
                error = "This field is required.";
                return false;
            }

            // Ensure the value satisfies all widget's constraints.
            var widget = Metadata.Widget();
            if (false == _metadata.IsReadOnly && false == widget.Validate(ValueProvider.Value, out error))
            {
                return false;
            }

            error = null;
            return true;
        }

        public IDataErrorInfo AsIDataErrorInfo()
        {
            IDataErrorInfo dataErrorInfo = this;
            return dataErrorInfo;
        }

        public ApplicationFieldDefinition Metadata
        {
            get { return _metadata; }
        }

        public bool IsValid
        {
            get
            {
                string error;
                return Validate(out error);
            }
        }

        /// <summary>
        ///     Gets or sets if validation is suppressed for this
        ///     binding. While suppressed, the binding is always
        ///     considered valid and calls to <see cref="IsValid"/>
        ///     or through <see cref="IDataErrorInfo"/> will always
        ///     return a valid state.
        /// </summary>
        /// <remarks>
        ///     This property setter is not intended to be part
        ///     of the public API of the component.
        /// </remarks>
        public bool IsValidationSuppressed { get; set; }

        public IValueProvider ValueProvider
        {
            get { return _valueProvider; }
        }

        string IDataErrorInfo.Error
        {
            get
            {
                string error;
                Validate(out error);
                return error;
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                IDataErrorInfo dataErrorInfo = this;
                return dataErrorInfo.Error;
            }
        }
    }
}