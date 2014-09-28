using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using softWrench.Mobile.Data;

using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.UI.Binding
{
    internal class FormBinding : IDataErrorInfo
    {       
        private readonly DataMap _dataMap;
        private readonly IList<FieldBinding> _fields;
        private readonly IList<CommandBinding> _commands;
        private bool _isValidationSuppressed;

        public FormBinding(DataMap dataMap, IList<FieldBinding> fields, IList<CommandBinding> commands, bool isNew)
        {
            _dataMap = dataMap;
            _fields = fields;
            _commands = commands;
            IsNew = isNew;
        }

        public FormBinding(DataMap dataMap, IList<FieldBinding> fields, IList<CommandBinding> commands) : this(dataMap, fields, commands, false)
        {
        }

        public int Commit()
        {
            var changes = 0;

            foreach (var field in _fields)
            {
                var oldValue = DataMap.Value(field.Metadata);
                var newValue = field.ValueProvider.Value;

                if (oldValue != newValue)
                {
                    DataMap.Value(field.Metadata, newValue);
                    changes++;
                }
            }

            return changes;
        }

        public IEnumerable<CommandBinding> AvailableCommands()
        {
            return Commands
                .Where(c => c.IsAvailable(this));
        }

        public IDataErrorInfo AsIDataErrorInfo()
        {
            IDataErrorInfo dataErrorInfo = this;
            return dataErrorInfo;
        }

        /// <summary>
        ///     Gets the data map backing the binding.
        /// </summary>
        public DataMap DataMap
        {
            get { return _dataMap; }
        }

        public FieldBinding Field(string attribute)
        {
            return _fields
                .First(c => c.Metadata.Attribute == attribute);
        }

        public FieldBinding Field(ApplicationFieldDefinition applicationField)
        {
            return Field(applicationField.Attribute);
        }

        public IList<FieldBinding> Fields
        {
            get { return _fields; }
        }

        public IList<CommandBinding> Commands
        {
            get { return _commands; }
        }

        public bool IsNew { get; set; }

        /// <summary>
        ///     Gets or sets if validation is suppressed for
        ///     this binding. While suppressed, the binding
        ///     is always considered valid and calls through
        ///     <see cref="IDataErrorInfo"/> will always return
        ///     a valid state.
        /// </summary>
        public bool IsValidationSuppressed
        {
            get { return _isValidationSuppressed; }
            set
            {
                _isValidationSuppressed = value;

                // Propagates the suppression
                // to all field bindings.
                foreach (var field in Fields)
                {
                    field.IsValidationSuppressed = value;
                }
            }
        }

        string IDataErrorInfo.Error
        {
            get
            {
                // If validation is suppressed, let's
                // take a shortcut and return no errors.
                if (IsValidationSuppressed)
                {
                    return null;
                }

                var firstError = _fields
                    .Cast<IDataErrorInfo>()
                    .FirstOrDefault(f => null != f.Error);

                return null == firstError
                    ? null
                    : String.Format("{0}: {1}", ((FieldBinding) firstError).Metadata.Label, firstError.Error);
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                // If validation is suppressed, let's
                // take a shortcut and return no errors.
                if (IsValidationSuppressed)
                {
                    return null;
                }

                IDataErrorInfo field = _fields
                    .First(f => f.Metadata.Attribute == columnName);

                return field.Error;
            }
        }
    }
}
