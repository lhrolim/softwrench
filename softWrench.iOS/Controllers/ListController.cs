using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using softWrench.iOS.UI.Eventing;
using softWrench.iOS.Utilities;
using softWrench.iOS.Views;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Persistence;
using softWrench.Mobile.Persistence.Expressions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.iOS.Controllers {
    public partial class ListController : BaseController {
        ApplicationSchemaDefinition _applicationMetadata;
        private UIBarButtonItem _hideMenuButton;
        private UISearchBar _searchBar;
        private AsyncSearch<List<DataMap>> _asyncSearch;
        private bool _wasLandscape = true;

        public ListController(IntPtr handle)
            : base(handle) {
        }

        public void Construct(ApplicationSchemaDefinition applicationSchemaDefinition) {
            _applicationMetadata = applicationSchemaDefinition;
        }

        public override void ViewDidLoad() {
            base.ViewDidLoad();

            var searchOptions = new AsyncSearchOptions<List<DataMap>>(SearchAsync, ReloadData);
            _asyncSearch = new AsyncSearch<List<DataMap>>(View, searchOptions);

            AddBorders();
            AddNavigationItems();
            AddSearchBar();
            SwitchOrientation(InterfaceOrientation);

            // TODO: this null check should be removed. If             
            // metadata is null, Reload should not called
            // in the first place.
            if (null != _applicationMetadata) {
                ReloadData();
            }
        }

        public override void ViewWillAppear(bool animated) {
            base.ViewWillAppear(animated);

            SubscribeToBus();
        }

        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration) {
            base.WillRotate(toInterfaceOrientation, duration);

            //Start an animation to switch orientations
            SwitchOrientation(toInterfaceOrientation);
        }

        public void ReloadSingleRow(NSIndexPath indexPath) {
            tableView.ReloadRows(new[] { indexPath }, UITableViewRowAnimation.Fade);
        }

          protected override void Dispose(bool disposing) {
            if (disposing) {
                UnsubscribeFromBus();
            }

            base.Dispose(disposing);
        }

        private void SubscribeToBus() {
            SimpleEventBus.Subscribe<PopoverMenuToggled>(OnPopoverMenuToggled);
            SimpleEventBus.Subscribe<DataMapSaved>(OnDataMapSaved);
        }

        private void UnsubscribeFromBus() {
            SimpleEventBus.Unsubscribe<PopoverMenuToggled>(OnPopoverMenuToggled);
            SimpleEventBus.Unsubscribe<DataMapSaved>(OnDataMapSaved);
        }

        private void AddBorders() {
            var rightBorder = new CALayer {
                Frame = new RectangleF(319, 0, 1f, tableView.Frame.Size.Height),
                BackgroundColor = Theme.BorderColor
            };
            View.Layer.AddSublayer(rightBorder);


        }

        private void AddNavigationItems() {
            NavigationItem.LeftItemsSupplementBackButton = true;

            _hideMenuButton = new UIBarButtonItem(Theme.MenuIcon(), UIBarButtonItemStyle.Plain, (sender, e) => OnTogglePopoverMenu()) {
                ImageInsets = new UIEdgeInsets(2, 0, -2, 0)
            };

            newButton.Clicked += OnNewButtonClick;

        }

        private void AddSearchBar() {
            _searchBar = new UISearchBar(new RectangleF(0, 0, tableView.Frame.Width, 44));
            _searchBar.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            _searchBar.Placeholder = "Search";
            _searchBar.AutocorrectionType = UITextAutocorrectionType.No;
            _searchBar.SearchButtonClicked += OnSearchButtonClicked;
            _searchBar.TextChanged += OnSearchTextChanged;

            var searchBarView = new UIView(new RectangleF(0, 0, tableView.Frame.Width, 44));
            searchBarView.AddSubview(_searchBar);
            tableView.TableHeaderView = searchBarView;
        }

        private Task<List<DataMap>> SearchAsync(string searchText) {
            var filters = string.IsNullOrWhiteSpace(searchText)
                ? Enumerable.Empty<FilterExpression>()
                : Enumerable.Repeat(new ContainsToken(searchText), 1);

            return new DataRepository()
                .LoadAsync(_applicationMetadata, filters);
        }

        private void ReloadData(List<DataMap> dataMaps) {
            tableView.Source = new TableSource(_applicationMetadata, dataMaps, OnDataMapSelected);
            tableView.ReloadData();
        }

        private async void ReloadData() {
            var dataMaps = await SearchAsync("");
            ReloadData(dataMaps);
        }

        private void SwitchOrientation(UIInterfaceOrientation orientation) {
            if (orientation.IsLandscape()) {
                if (!_wasLandscape) {
                    NavigationItem.SetLeftBarButtonItems(new UIBarButtonItem[0], true);
                    _wasLandscape = true;
                }
            } else {
                if (_wasLandscape) {
                    NavigationItem.SetLeftBarButtonItems(new[] { _hideMenuButton }, true);
                    _wasLandscape = false;
                }
            }
        }

        private void OnSearchTextChanged(object sender, UISearchBarTextChangedEventArgs e) {
            _asyncSearch.NotifySearchTextChanged(e.SearchText);
        }

        private void OnSearchButtonClicked(object sender, EventArgs e) {
            _searchBar.ResignFirstResponder();
            _asyncSearch.NotifySearchButtonClicked(_searchBar.Text);
        }

        private void OnTogglePopoverMenu() {
            SimpleEventBus.Publish(new PopoverMenuToggleRequested(false));
            NavigationItem.SetLeftBarButtonItems(new UIBarButtonItem[0], true);
        }

        private void OnPopoverMenuToggled(PopoverMenuToggled e) {
            if (false == e.Show) {
                return;
            }

            NavigationItem.SetLeftBarButtonItems(new[] { _hideMenuButton }, true);
        }

        /// <summary>
        ///     Invoked when a data map is selected on the screen.
        /// </summary>
        /// <param name="dataMap">The data map just selected.</param>
        private void OnDataMapSelected(DataMap dataMap) {
            var searchBar = _searchBar;
            if (null != searchBar) {
                searchBar.ResignFirstResponder();
            }

            SimpleEventBus.Publish(new DataMapSelected(dataMap, false));
        }

        private void OnDataMapSaved(DataMapSaved e) {
            ReloadData();
        }

        private void OnNewButtonClick(object sender, EventArgs e) {
            new DataRepository()
                .NewAsync(_applicationMetadata)
                .ContinueWith(t => SimpleEventBus.Publish(new DataMapSelected(t.Result, true)));
        }

      



        private class TableSource : UITableViewSource {
            private readonly ApplicationSchemaDefinition _applicationMetadata;
            private readonly IList<DataMap> _dataMap;
            private readonly Action<DataMap> _rowSelected;
            private string _lastSelectedId;

            /// <summary>
            ///     Creates a new instance of the <see cref="TableSource"/>
            ///     class using the specified values.
            /// </summary>
            /// <param name="applicationSchemaDefinition">The application metadata.</param>
            /// <param name="dataMap">The list of data maps backing the data source.</param>
            /// <param name="onDataMapSelected">The delegate invoked when a data map is selected on the screen.</param>
            public TableSource(ApplicationSchemaDefinition applicationSchemaDefinition, IList<DataMap> dataMap, Action<DataMap> onDataMapSelected) {
                _applicationMetadata = applicationSchemaDefinition;
                _dataMap = dataMap;
                _rowSelected = onDataMapSelected;
            }

            public override int RowsInSection(UITableView tableView, int section) {
                return _dataMap == null ? 0 : _dataMap.Count;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath) {
                var dataMap = _dataMap[_dataMap.Count - indexPath.Row - 1];
                var cell = (ListCell)tableView.DequeueReusableCell("ListCell");
                cell.Construct(_applicationMetadata, dataMap);

                if (dataMap.Id(_applicationMetadata) == _lastSelectedId) {
                    tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
                }
                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath) {
                var dataMap = _dataMap[_dataMap.Count - indexPath.Row - 1];

                // Let's remember the data map that is now selected
                // so we can highlight it again if the list reloads
                // for whatever reason.
                _lastSelectedId = dataMap.Id(_applicationMetadata);

                _rowSelected(dataMap);
            }
        }
    }
}
