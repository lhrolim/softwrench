using System;
using System.ComponentModel;
using MonoTouch.UIKit;
using softWrench.iOS.UI.Binding;
using softWrench.iOS.Utilities;

using softWrench.Mobile.Metadata.Applications.UI;
using softWrench.Mobile.Metadata.Extensions;
using softWrench.Mobile.UI.Binding;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.UI;

namespace softWrench.iOS.Views {
    public abstract class DetailFieldCellBase : UITableViewCell {
        private const float TextCellHeight = 44;
        private const float TextErrorCellHeight = 66;
        private const float LargeTextCellHeight = 132;
        private const float LargeTextErrorCellHeight = 154;
        private const float ImageCellHeight = 264;

        internal static bool UseLargeTextCell(FieldBinding binding) {
            //TODO: Ask Rolim for a "large text" widget and then get rid of this method.
            return "longdescription_.ldtext".Equals(binding.Metadata.Attribute,
                StringComparison.OrdinalIgnoreCase);
        }

        internal static bool UseImageCell(FieldBinding binding) {
            return binding.Metadata.Widget() is ImageWidgetDefinition;
        }

        /// <summary>
        ///     Evaluates the current state of the field binding
        ///     and decides the optimal row height for rendering.
        /// </summary>
        /// <param name="binding">The backing field binding.</param>
        internal static float GetHeightForRow(FieldBinding binding) {
            if (UseImageCell(binding)) {
                return ImageCellHeight;
            }

            if (UseLargeTextCell(binding)) {
                return binding.IsValid
                    ? LargeTextCellHeight
                    : LargeTextErrorCellHeight;
            }

            return binding.IsValid
                ? TextCellHeight
                : TextErrorCellHeight;
        }

        private bool _areUiEventsAttached;
        private ValueProvider _valueProvider;

        protected DetailFieldCellBase(IntPtr handle)
            : base(handle) {
        }

        internal virtual void Construct(ApplicationFieldDefinition metadata, ValueProvider valueProvider) {
            _valueProvider = valueProvider;
            Metadata = metadata;

            // We need to know when the control's text
            // changes, so we can relay the change to
            // our backing value provider.
            SubscribeToTextFieldChanges();

            // If other sources (e.g. programmatically)
            // change the value of the value provider,
            // we must propagate it to the cell.
            SubscribeToValueProviderChanges(valueProvider);

            Configure();

            // Initializes the cell with the
            // value provided by the binding.
            SyncFromValueProvider();
        }

        private void SyncFromValueProvider() {
            Text.Text = _valueProvider.Value;
        }


        private void SubscribeToTextFieldChanges() {
            if (_areUiEventsAttached) {
                return;
            }

            Text.ShouldReturn += ShouldReturn;

            _areUiEventsAttached = true;
        }

        private void SubscribeToValueProviderChanges(INotifyPropertyChanged valueProvider) {
            // If we were already listening to
            // one value provider, release it.
            if (null != _valueProvider) {
                _valueProvider.PropertyChanged -= OnValueProviderChanged;
            }

            valueProvider.PropertyChanged += OnValueProviderChanged;
        }

        private void OnValueProviderChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            SyncFromValueProvider();
        }

        private bool ShouldReturn(UITextField textInput) {
            // TODO: this will trigger an OnPropertyChanged,
            //       which will result on the cell value
            //       being set again (i.e. indirectly caused
            //       by itself).

            // TODO: this method is never invoked for UITextView
            //       (the multiline text widget for large texts).

            // Updates the binding with the
            // value offered by the the cell.
            SyncToValueProvider();

            textInput.ResignFirstResponder();
            return true;
        }

        private void ConfigureLabel() {
            Label.Text = (Metadata.IsRequired ? "* " : "") + Metadata.Label;

            Label.Font = Metadata.IsRequired
                ? Theme.BoldFontOfSize(Label.Font.PointSize)
                : Theme.FontOfSize(Label.Font.PointSize);
        }

        private void ConfigureText(WidgetConfiguration configuration) {
            Accessory = Metadata.IsReadOnly ? UITableViewCellAccessory.None : configuration.Accessory;
            Text.KeyboardType = configuration.KeyboardType;
            Text.UserInteractionEnabled = (false == Metadata.IsReadOnly) && configuration.UserInteractionEnabled;
            // If this is a read-only field,
            // use a different color.
            Text.TextColor = Metadata.IsReadOnly ? Theme.ReadOnlyTextColor : Theme.NonReadOnlyTextColor;
        }

        private void Configure() {
            // How should we configure the control?
            // As a decimal, as a lookup, ... ?
            var configuration = WidgetConfiguration.GetConfiguration(Metadata.Widget());

            // Is this cell the target of a lookup operation?
            IsLookup = Metadata.Widget() is LookupWidget;

            // Is this cell an image container?
            IsImage = Metadata.Widget() is ImageWidget;

            // Should we display a date picker when this cell is selected?
            IsDatePickerAvailable = configuration.IsDatePickerAvailable;

            ConfigureLabel();
            ConfigureText(configuration);
        }

        public void SyncToValueProvider() {
            _valueProvider.Value = Text.Text;
        }

        /// <summary>
        ///     Gets the underlying <see cref="UILabel"/>
        ///     that contains the field label.
        /// </summary>
        protected abstract UILabel Label { get; }

        /// <summary>
        ///     Gets the underlying <see cref="IUITextInput"/>
        ///     that contains the field value.
        /// </summary>
        protected abstract IUITextInput Text { get; }

        /// <summary>
        ///     Gets the field metadata.
        /// </summary>
        internal ApplicationFieldDefinition Metadata { get; private set; }

        /// <summary>
        ///     Gets whether the cell is the
        ///     target of a lookup operation.
        /// </summary>
        internal bool IsLookup { get; private set; }

        /// <summary>
        ///     Gets whether the cell contains
        ///     an image thumbnail.
        /// </summary>
        internal bool IsImage { get; private set; }

        /// <summary>
        ///     Gets whether the cell requests that a date picker
        ///     should be made available for input upon selection.
        /// </summary>
        internal bool IsDatePickerAvailable { get; private set; }

        /// <summary>
        ///     A helper class containing parameter sets
        ///     for the various cell behaviors available.
        /// </summary>
        private class WidgetConfiguration {
            private static WidgetConfiguration GetTextConfiguration() {
                return new WidgetConfiguration();
            }

            private static WidgetConfiguration GetNumberConfiguration() {
                return new WidgetConfiguration {
                    KeyboardType = UIKeyboardType.NumberPad
                };
            }

            private static WidgetConfiguration GetDateConfiguration() {
                return new WidgetConfiguration {
                    IsDatePickerAvailable = true,
                    KeyboardType = UIKeyboardType.NumbersAndPunctuation,
                    UserInteractionEnabled = false
                };
            }

            private static WidgetConfiguration GetLookupConfiguration() {
                return new WidgetConfiguration {
                    Accessory = UITableViewCellAccessory.DisclosureIndicator,
                    UserInteractionEnabled = false
                };
            }

            private static WidgetConfiguration GetImageConfiguration() {
                return new WidgetConfiguration {
                    Accessory = UITableViewCellAccessory.None,
                    UserInteractionEnabled = false
                };
            }

            public static WidgetConfiguration GetConfiguration(IWidget widget) {
                var numberWidget = widget as NumberWidget;
                if (null != numberWidget) {
                    return GetNumberConfiguration();
                }

                var dateWidget = widget as DateWidget;
                if (null != dateWidget) {
                    return GetDateConfiguration();
                }

                var lookupWidget = widget as LookupWidget;
                if (null != lookupWidget) {
                    return GetLookupConfiguration();
                }

                var imageWidget = widget as ImageWidget;
                if (null != imageWidget) {
                    return GetImageConfiguration();
                }

                return GetTextConfiguration();
            }

            private WidgetConfiguration() {
                Accessory = UITableViewCellAccessory.None;
                IsDatePickerAvailable = false;
                KeyboardType = UIKeyboardType.Default;
                UserInteractionEnabled = true;
            }

            public UITableViewCellAccessory Accessory { get; private set; }
            public bool IsDatePickerAvailable { get; private set; }
            public UIKeyboardType KeyboardType { get; private set; }
            public bool UserInteractionEnabled { get; private set; }
        }
    }
}