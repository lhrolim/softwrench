using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using softWrench.iOS.Behaviors;
using softWrench.iOS.UI.Binding;
using softWrench.iOS.Utilities;
using softWrench.iOS.Views;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Metadata.Extensions;
using softWrench.Mobile.Persistence;
using softWrench.Mobile.Persistence.Expressions;
using softWrench.Mobile.UI.Binding;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using UiBinder = softWrench.iOS.UI.Binding.UiBinder;
using softWrench.Mobile.Metadata.Applications.UI;

namespace softWrench.iOS.Controllers {
    public partial class DetailFormController : BaseTableController {
        private ApplicationSchemaDefinition _applicationMetadata;
        private DataMap _dataMap;
        private CompositeDataMap _composite;
        private FormBinding _binding;
        private DetailController _detailController;
        private Func<Task<bool>> _commandPreCondition;
        private Action<string> _titleSetter;
        private DetailDatePickerCell.Attachment _datePicker;
        private static readonly MetadataRepository MetadataRepository = MetadataRepository.GetInstance();

        public DetailFormController(IntPtr handle)
            : base(handle) {
        }

        internal void Construct(ApplicationSchemaDefinition application, DataMap dataMap, bool isNew, CompositeDataMap composite, DetailController detailController, Func<Task<bool>> commandPreCondition, Action<string> titleSetter) {
            _applicationMetadata = application;
            _dataMap = dataMap;
            _composite = composite;
            _detailController = detailController;
            _commandPreCondition = commandPreCondition;
            _titleSetter = titleSetter;

            // Create our buddy binding which will help
            // us to move data between the data map and
            // the UI controls.
            _binding = UiBinder.Bind(_dataMap, _applicationMetadata, isNew);

            // Load the data, but ignore validation for
            // now. We don't want objects that are already
            // invalid triggering a sea of red in the UI.
            _binding.IsValidationSuppressed = true;
        }

        private void SubscribeToBus() {

        }

        private void UnsubscribeFromBus() {
        }

        private void SyncToValueProviders() {
            // Notifies all cells to ensure their values (typically
            // held by an UITextField) are synchronized with their
            // underlying value provider.
            foreach (DetailFieldCellBase cell in TableView.VisibleCells.Where(c => c is DetailFieldCellBase)) {
                cell.SyncToValueProvider();
            }
        }

        private void ShowTitle() {
            const int maxLength = 25;

            if (_binding.IsNew) {
                _titleSetter(string.Format("New {0}", _applicationMetadata.Title));
                return;
            }

            // Do we have a field marked as the
            // "preview title" in our metadata?
            var previewTitle = _applicationMetadata
                .PreviewTitle();

            // If we don't, let's use the application
            // title then. Better than nothing, huh?
            if (null == previewTitle) {
                _titleSetter(_applicationMetadata.Title);
                return;
            }

            var title = _dataMap.Value(previewTitle.Attribute);

            // Damn, the field on the data map is
            // empty. Let's display a generic title.
            if (string.IsNullOrWhiteSpace(title)) {
                _titleSetter("Item");
                return;
            }

            // Truncates and add ellipses to
            // the text if its too large.
            if (title.Length > maxLength) {
                title = title.Substring(0, maxLength - 3) + "...";
            }

            // Can't believe we survived all if's.
            _titleSetter(title);
        }

        private FieldBinding GetField(NSIndexPath indexPath) {
            var indexPathBeforeOffset = null == _datePicker
                ? indexPath
                : _datePicker.IndexPathBeforeOffset(indexPath);

            return _binding
                .VisibleFields()
                .ElementAt(indexPathBeforeOffset.Row);
        }

        private CommandBinding GetCommand(NSIndexPath indexPath) {
            return _binding
                .AvailableCommands()
                .ElementAt(indexPath.Row);
        }

        private void ShowImage(DetailImageCell cell) {
            // If a field is currently being
            // edited by the user, we don't
            // want to lose this change.
            SyncToValueProviders();

            var widget = (ImageWidget)cell.Metadata.Widget();
            var title = _dataMap.Value("document");
            var image = _dataMap.Value(cell.Metadata);

            var imageController = Storyboard.InstantiateViewController<ImageController>();
            imageController.Construct(widget, title, image);
            NavigationController.PushViewController(imageController, true);
        }

        private void LookupImpl(ApplicationFieldDefinition fieldMetadata, Action<LookupController.Result> onCompletion, IEnumerable<FilterExpression> adHocFilters) {
            // If a field is currently being
            // edited by the user, we don't
            // want to lose this change.
            SyncToValueProviders();

            var lookupController = Storyboard.InstantiateViewController<LookupController>();
            lookupController.Construct(fieldMetadata, _dataMap, onCompletion, adHocFilters);
            NavigationController.PushViewController(lookupController, true);
        }

        private void Lookup(ApplicationFieldDefinition fieldMetadata, IEnumerable<FilterExpression> adHocFilters) {
            LookupImpl(fieldMetadata, OnLookupCompleted, adHocFilters);
        }

        private void OnLookupCompleted(LookupController.Result result) {
            // Which field is the target of the lookup,
            // i.e. the field that must be updated with
            // the value of the lookup source field?
            var target = _binding
                .Field(result.Widget.TargetField)
                .ValueProvider;

            // What value was just selected
            // in the lookup?
            var sourceValue = result
                .DataMap
                .Value(result.Widget.SourceField);

            target.Value = sourceValue;

            // Do we have to update all others
            // fields of the association?
            if (string.IsNullOrEmpty(result.Widget.TargetQualifier)) {
                return;
            }

            var qualifier = string.Format("{0}{1}", result.Widget.TargetQualifier, ApplicationFieldDefinition.AttributeQualifierSeparator);

            // All fields with the target qualifier should
            // be updated with the corresponding data contained
            // in the looked up entry.
            var qualifiedTargetFields = _applicationMetadata
                .Fields
                .Where(f => f.Attribute.StartsWith(qualifier));

            foreach (var field in qualifiedTargetFields) {
                var nonQualifiedAttribute = field
                    .Attribute
                    .Split(new[] { ApplicationFieldDefinition.AttributeQualifierSeparator }, StringSplitOptions.None)
                    .Last();

                target = _binding.Field(field).ValueProvider;
                target.Value = result.DataMap.Value(nonQualifiedAttribute);
            }
        }

        private void ToggleDatePicker(NSIndexPath target) {
            var shouldAttach = true;
            var targetBeforeOffset = target;

            TableView.BeginUpdates();

            // Do we already have a date picker being
            // displayed? If so, we need to get rid of it.
            if (null != _datePicker) {
                // We do have a date picker displayed! If it's attached
                // to another cell we'll detach it first, but we must
                // remember to re-attach ("move") it to the new target.
                // Otherwise we'll simply turn it off.
                shouldAttach = false == _datePicker.IsAttachedTo(target);

                // We'll soon detach the date picker from the table,
                // which can cause our current target path pointing
                // to somewhere else after committing the changes.
                // So we'll "convert" it to its original value, as
                // if the date picker didn't exist.
                targetBeforeOffset = _datePicker.IndexPathBeforeOffset(target);

                _datePicker.Detach();
                _datePicker = null;
            }

            if (shouldAttach) {
                var datePickerCell = CellTemplate.ConstructDatePickerCell(TableView);
                var targetField = GetField(targetBeforeOffset);

                _datePicker = DetailDatePickerCell.Attachment.Attach(TableView, targetBeforeOffset, targetField, datePickerCell);
            }

            TableView.EndUpdates();
        }

        private void Show() {
            // The moveable cell containing the
            // datepicker will start hidden.
            _datePicker = null;

            ShowTitle();
            TableView.ReloadData();
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                UnsubscribeFromBus();
            }

            base.Dispose(disposing);
        }

        public void OnSave() {
            // The item was saved!
            _binding.IsNew = false;

            // On save, refresh data to
            // clear validation messages.
            Show();
        }

        public bool Commit() {
            // Now the user explicitly demonstrated its
            // intention to save data, we'll start to
            // enforce validation.
            _binding.IsValidationSuppressed = false;

            // Ensures all visible cells pump their
            // current value to the data provider.
            // This ensures that cells that haven't
            // yet had time to invoke ShouldReturn,
            // EditingDidEnd, etc, have their value
            // collected anyway.
            SyncToValueProviders();

            var error = _binding
                .AsIDataErrorInfo()
                .Error;

            if (null != error) {
                using (var alert = new UIAlertView("Oops...", error, null, "OK", null)) {
                    alert.Show();
                }

                Show();
                return false;
            }

            _binding.Commit();
            return true;
        }

        public void Lookup(ApplicationFieldDefinition fieldMetadata, Action<LookupController.Result> onCompletion, IEnumerable<FilterExpression> adHocFilters) {
            if (fieldMetadata == null) throw new ArgumentNullException("fieldMetadata");
            if (onCompletion == null) throw new ArgumentNullException("onCompletion");
            if (adHocFilters == null) throw new ArgumentNullException("adHocFilters");

            // We accept completion handlers,
            // but we have our work to do too.
            var wrappedCompletion = new Action<LookupController.Result>(r => {
                                                                            OnLookupCompleted(r);
                                                                            onCompletion(r);
                                                                        });

            LookupImpl(fieldMetadata, wrappedCompletion, adHocFilters);
        }

        public override void ViewDidLoad() {
            base.ViewDidLoad();

            SubscribeToBus();
        }

        public override void ViewWillAppear(bool animated) {
            base.ViewWillAppear(animated);

            using (ActivityIndicator.Start(View)) {
                Show();
            }
        }

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath) {
            Theme.BorderHorizontally(cell, indexPath);
        }

        public override int NumberOfSections(UITableView tableView) {
            return 2;
        }

        public override int RowsInSection(UITableView tableView, int section) {
            if (null == _dataMap) {
                return 0;
            }

            // If the moveable cell containing the date picker
            // is being used, we have one extra row in the table.
            var datePickerOffset = (null == _datePicker ? 0 : 1);

            return section == 0
                ? _binding.VisibleFields().Count() + datePickerOffset
                : _binding.AvailableCommands().Count();
        }

        public override float GetHeightForRow(UITableView tableView, NSIndexPath indexPath) {
            if (indexPath.Section == 1) {
                var commandBinding = GetCommand(indexPath);
                return DetailCommandCell.GetHeightForRow(commandBinding);
            }

            // If the moveable cell containing the date picker
            // is being used (i.e. visible) we have to make
            // sure we don't confuse it with a field cell.
            if (null != _datePicker && _datePicker.IsDatePickerPath(indexPath)) {
                return DetailDatePickerCell.GetHeightForRow();
            }

            var fieldBinding = GetField(indexPath);
            return DetailFieldCellBase.GetHeightForRow(fieldBinding);
        }

        public override string TitleForHeader(UITableView tableView, int section) {
            if (section == 0) {
                return "Details";
            }

            return _binding.AvailableCommands().Any() ? "Actions" : "";
        }

        public override float GetHeightForHeader(UITableView tableView, int section) {
            if (section != 0) {
                return 66;
            }

            return 10;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath) {
            var cell = tableView.CellAt(indexPath) as DetailFieldCellBase;

            // No behavior will be carried on
            // if cell is marked as read-only.
            if (null == cell || cell.Metadata.IsReadOnly) {
                return;
            }

            if (cell.IsLookup) {
                Lookup(cell.Metadata, Enumerable.Empty<FilterExpression>());
                return;
            }

            if (cell.IsImage) {
                ShowImage((DetailImageCell)cell);
                return;
            }

            if (cell.IsDatePickerAvailable) {
                ToggleDatePicker(indexPath);
                return;
            }
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath) {
            if (null != _datePicker && _datePicker.IsDatePickerPath(indexPath)) {
                return _datePicker.Cell;
            }

            return indexPath.Section == 0
                ? GetCellForField(tableView, indexPath)
                : GetCellForCommand(tableView, indexPath);
        }

        private UITableViewCell GetCellForCommand(UITableView tableView, NSIndexPath indexPath) {
            var commandBinding = GetCommand(indexPath);

            var commandArguments = new ApplicationCommandArguments(
                _applicationMetadata, MetadataRepository, _dataMap, new DataRepository(), _composite, User.Current, _detailController);

            return CellTemplate.ConstructCommandCell(tableView, commandBinding, commandArguments, _commandPreCondition);
        }

        private UITableViewCell GetCellForField(UITableView tableView, NSIndexPath indexPath) {
            var fieldBinding = GetField(indexPath);
            return CellTemplate.ConstructFieldCell(tableView, fieldBinding);
        }

        /// <summary>
        ///     A helper class that contains constructors
        ///     for various cell templates.
        /// </summary>
        private static class CellTemplate {
            /// <summary>
            ///     Constructs a vanilla cell with a label
            ///     and a textbox in their default values.
            /// </summary>
            /// <param name="tableView">The parent <see cref="UITableView"/>.</param>
            /// <param name="fieldBinding">The field binding backing the cell.</param>
            private static DetailFieldCellBase ConstructFieldPlainCell(UITableView tableView, FieldBinding fieldBinding) {
                var cell = (DetailFieldCell)tableView.DequeueReusableCell("DetailFieldCell");
                cell.Construct(fieldBinding.Metadata, (ValueProvider)fieldBinding.ValueProvider);

                return cell;
            }

            /// <summary>
            ///     Constructs a cell with a label and a large textbox.
            /// </summary>
            /// <param name="tableView">The parent <see cref="UITableView"/>.</param>
            /// <param name="fieldBinding">The field binding backing the cell.</param>
            private static DetailFieldCellBase ConstructFieldLargeTextCell(UITableView tableView, FieldBinding fieldBinding) {
                var cell = (DetailFieldLargeTextCell)tableView.DequeueReusableCell("DetailFieldLargeTextCell");
                cell.Construct(fieldBinding.Metadata, (ValueProvider)fieldBinding.ValueProvider);

                return cell;
            }

            /// <summary>
            ///     Constructs a cell with a label and a textbox
            ///     in their default values, in an invalid input
            ///     state.
            /// </summary>
            /// <param name="tableView">The parent <see cref="UITableView"/>.</param>
            /// <param name="fieldBinding">The field binding backing the cell.</param>
            private static DetailFieldCellBase ConstructFieldPlainErrorCell(UITableView tableView, FieldBinding fieldBinding) {
                var cell = (DetailFieldErrorCell)tableView.DequeueReusableCell("DetailFieldErrorCell");
                cell.Construct(fieldBinding.Metadata, (ValueProvider)fieldBinding.ValueProvider);

                cell.Message = fieldBinding
                    .AsIDataErrorInfo()
                    .Error;

                return cell;
            }

            /// <summary>
            ///     Constructs a cell with a label and a large
            ///     textbox, in an invalid input state.
            /// </summary>
            /// <param name="tableView">The parent <see cref="UITableView"/>.</param>
            /// <param name="fieldBinding">The field binding backing the cell.</param>
            private static DetailFieldCellBase ConstructFieldLargeTextErrorCell(UITableView tableView, FieldBinding fieldBinding) {
                var cell = (DetailFieldLargeTextErrorCell)tableView.DequeueReusableCell("DetailFieldLargeTextErrorCell");
                cell.Construct(fieldBinding.Metadata, (ValueProvider)fieldBinding.ValueProvider);

                cell.Message = fieldBinding
                    .AsIDataErrorInfo()
                    .Error;

                return cell;
            }

            /// <summary>
            ///     Constructs a cell containing an image thumbnail.
            /// </summary>
            /// <param name="tableView">The parent <see cref="UITableView"/>.</param>
            /// <param name="fieldBinding">The field binding backing the cell.</param>
            private static DetailFieldCellBase ConstructImageCell(UITableView tableView, FieldBinding fieldBinding) {
                var cell = (DetailImageCell)tableView.DequeueReusableCell("DetailImageCell");
                cell.Construct(fieldBinding.Metadata, (ValueProvider)fieldBinding.ValueProvider);

                return cell;
            }

            /// <summary>
            ///     Evaluates the binding state and decides
            ///     which cell template must be constructed.
            /// </summary>
            /// <param name="tableView">The parent <see cref="UITableView"/>.</param>
            /// <param name="fieldBinding">The field binding backing the cell.</param>
            public static DetailFieldCellBase ConstructFieldCell(UITableView tableView, FieldBinding fieldBinding) {
                if (DetailFieldCellBase.UseImageCell(fieldBinding)) {
                    return ConstructImageCell(tableView, fieldBinding);
                }

                if (DetailFieldCellBase.UseLargeTextCell(fieldBinding)) {
                    return fieldBinding.IsValid
                        ? ConstructFieldLargeTextCell(tableView, fieldBinding)
                        : ConstructFieldLargeTextErrorCell(tableView, fieldBinding);
                }

                return fieldBinding.IsValid
                    ? ConstructFieldPlainCell(tableView, fieldBinding)
                    : ConstructFieldPlainErrorCell(tableView, fieldBinding);
            }

            /// <summary>
            ///     Constructs a cell containing a large button
            ///     that represents an UI command available to
            ///     the user.
            /// </summary>
            /// <param name="tableView">The parent <see cref="UITableView"/>.</param>
            /// <param name="commandBinding">The command binding backing the cell.</param>
            /// <param name="commandArguments">The command arguments to be dispatched when the command is invoked by the user.</param>
            /// <param name="commandPreCondition">A delegate to be evaluated before executing the command.</param>
            public static UITableViewCell ConstructCommandCell(UITableView tableView, CommandBinding commandBinding, ApplicationCommandArguments commandArguments, Func<Task<bool>> commandPreCondition) {
                var cell = (DetailCommandCell)tableView.DequeueReusableCell("DetailCommandCell");
                cell.Construct(commandBinding.Command, commandArguments, commandPreCondition);

                return cell;
            }

            /// <summary>
            ///     Constructs a cell with a date picker that
            ///     can be visually attached to another, thus
            ///     redirecting its input to the target cell.
            /// </summary>
            /// <param name="tableView">The parent <see cref="UITableView"/>.</param>
            public static DetailDatePickerCell ConstructDatePickerCell(UITableView tableView) {
                var cell = (DetailDatePickerCell)tableView.DequeueReusableCell("DetailDatePickerCell");
                cell.Construct();

                return cell;
            }
        }
    }
}
