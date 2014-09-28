using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using softWrench.Mobile.Metadata.Applications.UI;
using softWrench.Mobile.Metadata.Extensions;
using softWrench.Mobile.UI.Binding;

namespace softWrench.iOS.Views
{
    public partial class DetailDatePickerCell : UITableViewCell
    {
        public static float GetHeightForRow()
        {
            return 216;
        }

        private bool _isConstructed;
        private FieldBinding _target;

        public DetailDatePickerCell (IntPtr handle) : base (handle)
        {
        }

        public void Construct()
        {
            if (false == _isConstructed)
            {
                // We need to know when the value of the
                // date picker changes so we can relay it
                // to the (attachment) target cell.
                picker.ValueChanged += OnDatePickerValueChanged;
                
                _isConstructed = true;
            }
        }

        private void OnDatePickerValueChanged(object sender, EventArgs eventArgs)
        {
            var target = _target;

            if (null == target)
            {
                return;
            }

            var widget = _target
                .Metadata
                .Widget();

            // Dispatches the current value of the date
            // picker to the target value provider.
            var date = picker.Date.ToString();
            target.ValueProvider.Value = widget.Format(date);
        }

        private void Configure(FieldBinding binding)
        {
            var widget = (DateWidget) binding
                .Metadata
                .Widget();

            picker.Mode = widget.Time
                ? UIDatePickerMode.Time
                : UIDatePickerMode.Date;

            DateTime currentValue;
            if (DateTime.TryParse(binding.ValueProvider.Value, out currentValue))
            {
                picker.SetDate(currentValue, false);
            }
        }

        private void Attach(FieldBinding binding)
        {
            _target = binding;

            Configure(binding);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                picker.ValueChanged -= OnDatePickerValueChanged;
            }

            _target = null;
            base.Dispose(disposing);
        }

        internal class Attachment
        {
            private static NSIndexPath IndexPathForAttachment(NSIndexPath target)
            {
                // The datepicker should be positioned
                // immediately below the target cell.
                return NSIndexPath.FromRowSection(target.Row + 1, target.Section);
            }
            
            public static Attachment Attach(UITableView tableView, NSIndexPath toIndexPath, FieldBinding toField, DetailDatePickerCell datePicker)
            {
                if (tableView == null) throw new ArgumentNullException("tableView");
                if (toIndexPath == null) throw new ArgumentNullException("toIndexPath");
                if (toField == null) throw new ArgumentNullException("toField");
                if (datePicker == null) throw new ArgumentNullException("datePicker");

                var datePickerPath = IndexPathForAttachment(toIndexPath);
                tableView.InsertRows(new[] { datePickerPath }, UITableViewRowAnimation.Fade);
                datePicker.Attach(toField);

                return new Attachment(tableView, toIndexPath, datePicker, datePickerPath);
            }

            private readonly UITableView _tableView;
            private readonly NSIndexPath _target;
            private readonly DetailDatePickerCell _cell;
            private readonly NSIndexPath _datePickerPath;

            private Attachment(UITableView tableView, NSIndexPath target, DetailDatePickerCell cell, NSIndexPath path)
            {
                _tableView = tableView;
                _target = target;
                _cell = cell;
                _datePickerPath = path;
            }

            public void Detach()
            {
                _tableView.DeleteRows(new[] { _datePickerPath }, UITableViewRowAnimation.Fade);
            }

            public NSIndexPath IndexPathBeforeOffset(NSIndexPath indexPathAfterOffset)
            {
                if (indexPathAfterOffset.Row < _datePickerPath.Row)
                {
                    return indexPathAfterOffset;
                }

                if (indexPathAfterOffset.Row > _datePickerPath.Row)
                {
                    return NSIndexPath.FromRowSection(indexPathAfterOffset.Row - 1, indexPathAfterOffset.Section);
                }

                // TODO: throw?
                return null;
            }

            public bool IsAttachedTo(NSIndexPath maybeTarget)
            {
                return _target.Compare(maybeTarget) == (int) NSComparisonResult.Same;
            }

            public bool IsDatePickerPath(NSIndexPath maybePath)
            {
                return _datePickerPath.Compare(maybePath) == (int) NSComparisonResult.Same;
            }

            public UITableViewCell Cell
            {
                get { return _cell; }
            }
        }
    }
}