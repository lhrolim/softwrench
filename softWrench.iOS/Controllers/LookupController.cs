using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Applications;

using softWrench.Mobile.Metadata.Applications.UI;
using softWrench.Mobile.Metadata.Extensions;
using softWrench.Mobile.Persistence;
using softWrench.Mobile.Persistence.Expressions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.iOS.Controllers
{
    public partial class LookupController : BaseController
	{
        private static IEnumerable<FilterExpression> InitializeFilters(DataMap dataMap, LookupWidget widget, IEnumerable<FilterExpression> adHocFilters)
        {
            var filters = new List<FilterExpression>();

            // Handles the metadata filters, which
            // can be either literals ("hardcoded")
            // or based on the data map current state.
            foreach (var filter in widget.Filters)
            {
                var value = string.IsNullOrEmpty(filter.Literal)
                    ? dataMap.Value(filter.TargetField)
                    : filter.Literal;

                filters.Add(new Exactly(filter.SourceField, value));
            }

            // Now let's append all ad-hoc filter.
            filters.AddRange(adHocFilters);

            return filters;
        }
       
        private ApplicationFieldDefinition _field;
        private LookupWidget _widget;
        private DataMap _dataMap;
        private Action<Result> _onCompletion;
        private ApplicationSchemaDefinition _lookupApplication;
        private AsyncSearch<List<DataMap>> _asyncSearch;
        private IEnumerable<FilterExpression> _filters;
        private UISearchBar _searchBar;

        public LookupController(IntPtr handle) : base (handle)
		{
		}

        public void Construct(ApplicationFieldDefinition field, DataMap dataMap, Action<Result> onCompletion, IEnumerable<FilterExpression> adHocFilters)
        {
            if (field == null) throw new ArgumentNullException("field");
            if (dataMap == null) throw new ArgumentNullException("dataMap");
            if (onCompletion == null) throw new ArgumentNullException("onCompletion");
            if (adHocFilters == null) throw new ArgumentNullException("adHocFilters");

            _field = field;
            _widget = (LookupWidget) field.Widget();
            _dataMap = dataMap;
            _onCompletion = onCompletion;
            _filters = InitializeFilters(_dataMap, _widget, adHocFilters);
        }

        public void Construct(ApplicationFieldDefinition field, DataMap dataMap, Action<Result> onCompletion)
        {
            Construct(field, dataMap, onCompletion,Enumerable.Empty<FilterExpression>());
        }

        private void AddSearchBar()
        {
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

        private void OnSearchTextChanged(object sender, UISearchBarTextChangedEventArgs e)
        {
            _asyncSearch.NotifySearchTextChanged(e.SearchText);
        }

        private void OnSearchButtonClicked(object sender, EventArgs e)
        {
            _searchBar.ResignFirstResponder();
            _asyncSearch.NotifySearchButtonClicked(_searchBar.Text);
        }

        private Task<List<DataMap>> SearchAsync(string searchText)
        {
            var filters = string.IsNullOrWhiteSpace(searchText)
                ? _filters
                : _filters.Concat(Enumerable.Repeat(new ContainsToken(searchText), 1));

            return new DataRepository()
                .LoadAsync(_lookupApplication, filters);
        }

        private void ReloadData(List<DataMap> dataMaps)
        {
            ((TableSource)tableView.Source).DataMaps = dataMaps;
            tableView.ReloadData();
        }

        private void OnCompletion(Result result)
        {
            // Before popping the navigation stack,
            // let's store our completion handler
            // to avoid losing it by cleanup methods. 
            var onCompletion = _onCompletion;

            NavigationController.PopViewControllerAnimated(true);

            if (null == onCompletion)
            {
                return;
            }

            onCompletion(result);

            // We don't want to keep unnecessary references
            // to (possibly) other controllers.
            _onCompletion = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var asyncSearch = _asyncSearch;
                if (null != asyncSearch)
                {
                    asyncSearch.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var searchOptions = new AsyncSearchOptions<List<DataMap>>(SearchAsync, ReloadData);
            _asyncSearch = new AsyncSearch<List<DataMap>>(View, searchOptions);

            tableView.Source = new TableSource(this);

            Title = _field.Label;
            AddSearchBar();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            //TODO: the entity is being used as
            //the application name.
            MetadataRepository.GetInstance()
                .LoadApplicationAsync(_widget.SourceApplication)
                .ContinueWith(t =>
                                  {
                                        _lookupApplication = t.Result;
                                        return SearchAsync("");
                                  })
                .Unwrap()
                .ContinueWith(t => InvokeOnMainThread(() => ReloadData(t.Result)));
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            // Releases the completion handler to
            // avoid hanging other controllers.
            _onCompletion = null;
        }

        private sealed class TableSource : UITableViewSource
        {
            private readonly LookupController _controller;
            private const string Identifier = "LookupCell";

            public TableSource(LookupController controller)
            {
                _controller = controller;
            }

            private DataMap GetDataMap(NSIndexPath indexPath)
            {
                return DataMaps[indexPath.Row];
            }

            private void ShowDataMap(UITableViewCell cell, DataMap dataMap)
            {
                var display = _controller
                    ._widget
                    .SourceDisplay
                    .ToArray();

                if (display.Length > 0)
                {
                    cell.TextLabel.Text = dataMap.Value(display[0]);
                }

                if (display.Length > 1)
                {
                    cell.DetailTextLabel.Text = dataMap.Value(display[1]);
                }
            }

            public override int RowsInSection(UITableView tableview, int section)
            {
                return null == DataMaps ? 0 : DataMaps.Count;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var dataMap = GetDataMap(indexPath);
                var cell = tableView.DequeueReusableCell(Identifier) ??
                    new UITableViewCell(UITableViewCellStyle.Subtitle, Identifier);

                ShowDataMap(cell, dataMap);

                return cell;
            }

            public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
            {
                Theme.BorderHorizontally(cell, indexPath);
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var field = _controller._field;
                var widget = _controller._widget;
                var dataMap = GetDataMap(indexPath);

                _controller.OnCompletion(new Result(field, widget, dataMap));
            }

            public List<DataMap> DataMaps { private get; set; }
        }

        public class Result
        {
            private readonly ApplicationFieldDefinition _field;
            private readonly LookupWidget _widget;
            private readonly DataMap _dataMap;

            public Result(ApplicationFieldDefinition field, LookupWidget widget, DataMap dataMap)
            {
                _field = field;
                _widget = widget;
                _dataMap = dataMap;
            }

            public DataMap DataMap
            {
                get { return _dataMap; }
            }

            public ApplicationFieldDefinition Field
            {
                get { return _field; }
            }

            public LookupWidget Widget
            {
                get { return _widget; }
            }
        }
    }
}
