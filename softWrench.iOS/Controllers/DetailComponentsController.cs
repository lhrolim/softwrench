using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Data;

using softWrench.Mobile.Metadata.Extensions;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;

namespace softWrench.iOS.Controllers {
    public partial class DetailComponentsController : BaseTableController {
        private static void SetText(UILabel label, string text) {
            const int maxLength = 50;

            if (null == text) {
                label.Text = "";
                return;
            }

            // Truncates and add ellipses to
            // the text if its too large.
            if (text.Length > maxLength) {
                text = text.Substring(0, maxLength - 3) + "...";
            }

            label.Text = text;
        }

        private ApplicationCompositionDefinition _composition;
        private CompositeDataMap _compositeDataMap;
        private IReadOnlyList<DataMap> _componentDataMaps;
        private DetailController _detailController;

        public DetailComponentsController(IntPtr handle)
            : base(handle) {
        }

        internal void Construct(ApplicationCompositionDefinition composition, CompositeDataMap dataMap, DetailController detailController) {
            _composition = composition;
            _compositeDataMap = dataMap;
            _componentDataMaps = dataMap.Components(composition.To());
            _detailController = detailController;
        }

        private void SubscribeToBus() {

        }

        private void UnsubscribeFromBus() {
        }

        private UITableViewCell ConstructCell(UITableView tableView, DataMap dataMap) {
            var cell = tableView.DequeueReusableCell("DetailComponentsCell");

            if (null != _composition.To().PreviewTitle()) {
                SetText(cell.TextLabel, dataMap.Value(_composition
                    .To()
                    .PreviewTitle()
                    .Attribute));
            } else {
                SetText(cell.TextLabel, "");
            }

            if (null != _composition.To().PreviewSubtitle()) {
                SetText(cell.DetailTextLabel, dataMap.Value(_composition
                    .To()
                    .PreviewSubtitle()
                    .Attribute));
            } else {
                SetText(cell.DetailTextLabel, "");
            }

            return cell;
        }

        private void NavigateToComponent(DataMap dataMap, bool isNew) {
            var controller = Storyboard.InstantiateViewController<DetailComponentNavigationController>();
            controller.Construct(_composition.To(), dataMap, isNew, _compositeDataMap, _detailController, OnComponentCompletion);

            controller.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
            controller.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
            NavigationController.PresentViewController(controller, true, null);
        }

        private void OnComponentCompletion(DetailComponentController.Result result) {
            // If the view was canceled, let's
            // simply de-select the current row.
            if (false == result.IsSuccess) {
                var selectedRow = TableView.IndexPathForSelectedRow;
                if (null != selectedRow) {
                    TableView.DeselectRow(selectedRow, true);
                }
                return;
            }

            if (result.IsNew) {
                _compositeDataMap.AddComponent(_composition.To(), result.DataMap);
            }

            TableView.ReloadData();
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                UnsubscribeFromBus();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Navigates to the details of a new component, created on
        ///     the fly with the default values. Upon user confirmation,
        ///     it's appended to the table.
        /// </summary>
        public async void NavigateToNewComponent() {
            var dataMap = await new DataRepository()
                .NewAsync(_composition.To(), _compositeDataMap);

            NavigateToComponent(dataMap, true);
        }

        /// <summary>
        ///     Navigates to the details of a new component initialized
        ///     with the specified values. Upon user confirmation, it's
        ///     appended to the table.
        /// </summary>
        /// <param name="dataMap">The data map of the new component containing its initial values.</param>
        public void NavigateToNewComponent(DataMap dataMap) {
            NavigateToComponent(dataMap, true);
        }

        public override void ViewDidLoad() {
            base.ViewDidLoad();

            SubscribeToBus();
            TableView.ReloadData();
        }

        public override string TitleForHeader(UITableView tableView, int section) {
            return _composition
                .To()
                .Title;
        }

        public override int RowsInSection(UITableView tableView, int section) {
            return null == _componentDataMaps ? 0 : _componentDataMaps.Count();
        }

        public override float GetHeightForHeader(UITableView tableView, int section) {
            return 10;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath) {
            var dataMap = _componentDataMaps[indexPath.Row];
            return ConstructCell(tableView, dataMap);
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath) {
            var dataMap = _componentDataMaps[indexPath.Row];
            NavigateToComponent(dataMap, false);
        }
    }
}
