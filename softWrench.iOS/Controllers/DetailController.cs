using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using softWrench.iOS.UI.Eventing;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Applications;

using softWrench.Mobile.Metadata.Extensions;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.iOS.Controllers {
    public partial class DetailController : BaseNavigationRootController {
        private CompositeDataMap _dataMap;
        private UIViewController _selectedController;
        private DetailFormController _compositeController;

        /// <summary>
        ///     The list of controllers for all component ("child")
        ///     applications, if this is a composite relationship.
        ///     The controllers are stored with the corresponding
        ///     metadata of the child application.
        /// </summary>
        private List<KeyValuePair<ApplicationSchemaDefinition, DetailComponentsController>> _componentControllers;
        private UIBarButtonItem _saveButton;

        private UIBarButtonItem _previousButton;
        private UIBarButtonItem _previousSpaceButton;
        private UIBarButtonItem _nextSpaceButton;
        private UIBarButtonItem _nextButton;
        private UIBarButtonItem _newButton;

        public DetailController(IntPtr handle)
            : base(handle) {
        }

        public void Construct(CompositeDataMap dataMap, bool isNew) {
            _dataMap = dataMap;

            ConstructChildControllers(isNew);
        }

        private void ConstructChildControllers(bool isNew) {
            // No controller is yet selected.
            _selectedController = null;

            // Creates the controller that
            // contains the actual form.
            _compositeController = Storyboard.InstantiateViewController<DetailFormController>();
            _compositeController.Construct(_dataMap.Application, _dataMap.Composite, isNew, _dataMap, this, OnCommand, t => Title = t);
            AddChildViewController(_compositeController);
            _compositeController.DidMoveToParentViewController(this);

            _componentControllers = new List<KeyValuePair<ApplicationSchemaDefinition, DetailComponentsController>>();

            // Creates one controller for each
            // component of the composition.
            foreach (var composition in _dataMap.Application.Compositions) {
                var controller = Storyboard.InstantiateViewController<DetailComponentsController>();
                controller.Construct(composition, _dataMap, this);
                AddChildViewController(controller);
                controller.DidMoveToParentViewController(this);

                // "Index" the controller by
                // its application metadata.
                var component = new KeyValuePair<ApplicationSchemaDefinition, DetailComponentsController>(composition.To(), controller);
                _componentControllers.Add(component);
            }
        }

        private void SubscribeToBus() {

        }

        private void UnsubscribeFromBus() {
        }

        private void InitializeSegments() {
            const float segmentWidth = 44 * 3;

            var previousCenter = segmentedControl.Center;
            segmentedControl.RemoveAllSegments();

            // Let's hide the segmented control if
            // the application is not composed.
            if (false == _dataMap.Application.Compositions.Any()) {
                segmentedControl.Hidden = true;
                return;
            }

            var i = 0;

            // This is the segment for the composite
            // (i.e. parent) application.
            segmentedControl.InsertSegment("Details", i++, false);

            // And now one segment for each component
            // (i.e. child) application.
            foreach (var composition in _dataMap.Application.Compositions) {
                var metadata = composition.To();
                segmentedControl.InsertSegment(metadata.Title, i++, false);
            }

            var newWidth = segmentWidth * i;
            segmentedControl.Frame = new RectangleF(previousCenter.X - (newWidth / 2), segmentedControl.Frame.Y + 20, newWidth, segmentedControl.Frame.Height);
            segmentedControl.Hidden = false;
            segmentedControl.SelectedSegment = 0;
            segmentedControl.ValueChanged += OnSegmentedControlChanged;

            // Is it just me or the segmented control labels appear
            // to be vertically misaligned using the default offset
            // provided by the system (iOS7)?
            for (var s = 0; s < segmentedControl.NumberOfSegments; s++) {
                segmentedControl.SetContentOffset(new SizeF(0, 1), s);
            }
        }

        private void WarnIfBouncing() {
            if (false == _dataMap.Composite.LocalState.IsBouncing) {
                // If the item has no sync errors,
                // shrinks the header to save space.
                segmentedControl.Frame = segmentedControl.Frame.Resize(y: 44f);
                newCompositionButton.Frame = newCompositionButton.Frame.Resize(y: 76f);
                containerView.Frame = containerView.Frame.Resize(y: 116f, height: 116f + containerView.Frame.Height);
                bounceReason.Hidden = true;

                return;
            }

            segmentedControl.Frame = segmentedControl.Frame.Resize(y: 88f);
            newCompositionButton.Frame = newCompositionButton.Frame.Resize(y: 120f);
            containerView.Frame = containerView.Frame.Resize(y: 160f, height: 160f + containerView.Frame.Height);
            bounceReason.Hidden = false;

            var reason = _dataMap
                .Composite
                .LocalState
                .BounceReason;

            var buffer = "The changes made on this item could not be synchronized with the server. " +
                (reason ?? "If this is a recurring problem, please contact your system administrator.");

            bounceReason.Text = buffer;
        }

        private void InitializeHeader() {
            InitializeSegments();
            WarnIfBouncing();
        }

        private void ShowSegmentImpl(int segment) {
            if (null == _dataMap) {
                return;
            }

            var rightButtons = new List<UIBarButtonItem> { _saveButton, _nextSpaceButton, _nextButton };

            // Activates the controller which contains the
            // details of the composite (i.e. parent) app.
            if (segment == 0) {
                _selectedController = _compositeController;
                newCompositionButton.Hidden = true;
            } else {
                _selectedController = _componentControllers[segment - 1].Value;

                // Let's display the "new child"
                // button, but only if the data
                // map already exists on the server.
                if (!_dataMap.Composite.LocalState.IsLocal) {
                    //                    buttons.Add(_newButton);
                }
                newCompositionButton.Hidden = false;
            }

            Theme.Fade(View);
            segmentedControl.SelectedSegment = segment;

            // Performs the view transfer dance.
            _selectedController.View.Frame = containerView.Bounds;
            containerView.AddSubview(_selectedController.View);
            _selectedController.View.MovedToSuperview();

            var originalLeftButtons = NavigationItem.LeftBarButtonItems;
            var leftButtons = originalLeftButtons == null
                ? new[] { _previousSpaceButton, _previousButton }
                : originalLeftButtons.Concat(new[] { _previousSpaceButton, _previousButton }).ToArray();

            NavigationItem.SetLeftBarButtonItems(leftButtons
                , false);
            NavigationItem.SetRightBarButtonItems(rightButtons.ToArray(), false);


        }

        private async Task SaveAndDisplaySuccessAlertAsync() {
            var success = await SaveAsync();

            if (success) {
                Alert.Show("Saved", "Your work was saved.");
            }
        }

        private async void OnSaveTouchUpInside(object sender, EventArgs e) {
            await SaveAndDisplaySuccessAlertAsync();
        }

        private void OnNewTouchUpInside(object sender, EventArgs e) {
            var controller = _selectedController as DetailComponentsController;

            if (null == controller) {
                return;
            }

            // Instruct the child controller to
            // trigger the "new child" workflow.
            controller.NavigateToNewComponent();
        }

        private void OnSegmentedControlChanged(object sender, EventArgs e) {
            ShowSegmentImpl(segmentedControl.SelectedSegment);
        }

        private Task<bool> OnCommand() {
            // Before executing a command
            // we'll save the current data.
            return Task.Factory.StartNew(() => {
                Task<bool> save = null;
                InvokeOnMainThread(() => save = SaveAsync());
                return save.Result;
            });
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                UnsubscribeFromBus();
                segmentedControl.ValueChanged -= OnSegmentedControlChanged;

                var imageView = View as UIImageView;
                if (null != imageView) {
                    imageView.Image.Dispose();
                    imageView.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public async Task<bool> SaveAsync() {
            if (null == _dataMap) {
                return false;
            }

            using (ActivityIndicator.Start(View)) {
                if (false == _compositeController.Commit()) {
                    ShowSegmentImpl(0);
                    return false;
                }

                // TODO: should we reload the table view
                // to refresh data potentially changed by
                // the OnBeforeSave pipeline?
                await new DataRepository().SaveAsync(_dataMap);

                // Refresh our form to update validation
                // status, commands availability, ...
                _compositeController.OnSave();

                SimpleEventBus.Publish(new DataMapSaved(_dataMap.Composite));
            }

            return true;
        }

        /// <summary>
        ///     Switches the active segment ("tab") of the controller's segmented
        ///     control to the segment corresponding to the specified application.
        /// </summary>
        /// <param name="application">The application name.</param>
        public void ShowSegment(string application) {
            // If we're requested to display the segment
            // of the composite application, it's always
            // located on the first segment.
            if (_dataMap.Application.Name == application) {
                ShowSegmentImpl(0);
                return;
            }

            // Nope. Let's assume the metadata pertains
            // to a component application then. We only
            // need to find out which one.
            var segment = _componentControllers
                .FindIndex(f => f.Key.Name == application);

            // The first segment is reserved to the
            // component, i.e. "parent", application.
            segment++;

            ShowSegmentImpl(segment);
        }

        /// <summary>
        ///     Switches the active segment ("tab") of the controller's segmented
        ///     control to the segment corresponding to the specified application,
        ///     then displays the "new" dialog filled with the specifed data map.
        /// </summary>
        /// <param name="application">The application name.</param>
        /// <param name="newDataMap">The data map to be displayed by the "new" dialog.</param>
        public void ShowSegment(string application, DataMap newDataMap) {
            if (application == null) throw new ArgumentNullException("application");
            if (newDataMap == null) throw new ArgumentNullException("newDataMap");
            if (_dataMap.Application.Name == application) {
                //TODO: throw, new data maps are only supported for components, not composite root.
                return;
            }

            // Activates the target segment...
            ShowSegment(application);

            // ... and now triggers the "new" dialog,
            // filled with the specified data map.
            ((DetailComponentsController)_selectedController).NavigateToNewComponent(newDataMap);
        }

        public override void ViewDidLoad() {
            base.ViewDidLoad();

            if (null == _dataMap) {
                return;
            }

            _saveButton = new UIBarButtonItem(UIBarButtonSystemItem.Save, OnSaveTouchUpInside);
            //            _saveButton.SetBackgroundImage(Theme.NavButton,UIControlState.Normal,UIBarMetrics.Default);

            // Prepares the "new child" button.
            _newButton = new UIBarButtonItem(UIBarButtonSystemItem.Add, OnNewTouchUpInside) {
                Style = UIBarButtonItemStyle.Bordered,

            };


            _previousButton = new UIBarButtonItem(UIBarButtonSystemItem.Rewind, OnPreviousClicked);
            _previousButton.Enabled = _dataMap.Composite.Previous != null;

            //TODO: adjust size dinamically
            _previousSpaceButton = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 200 };
            _nextSpaceButton = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 200 };

            _nextButton = new UIBarButtonItem(UIBarButtonSystemItem.FastForward, OnNextClicked);
            _nextButton.Enabled = _dataMap.Composite.Next != null;

            newCompositionButton.SetBackgroundImage(Theme.CommandButton, UIControlState.Normal);
            newCompositionButton.AllTouchEvents += OnNewTouchUpInside;
            _saveButton.Clicked += OnSaveTouchUpInside;


            SubscribeToBus();
            InitializeHeader();
            ShowSegmentImpl(0);
        }

        private void OnPreviousClicked(object sender, EventArgs e) {
            SimpleEventBus.Publish(new DataMapSelected(_dataMap.Composite.Previous, false));
        }

        private void OnNextClicked(object sender, EventArgs e) {
            SimpleEventBus.Publish(new DataMapSelected(_dataMap.Composite.Next, false));
        }

        private void OnPopoverMenuToggled(PopoverMenuToggleRequested e) {
            if (e.Show) {
                return;
            }
            var leftBarButtonItems = NavigationItem.LeftBarButtonItems;
            if (NavigationItem != null && leftBarButtonItems != null) {
                var uiBarButtonItems = leftBarButtonItems.Concat(new[] { _previousSpaceButton, _previousButton }).ToArray();
                NavigationItem.SetLeftBarButtonItems(uiBarButtonItems, true);
            }
        }

        public override void ViewWillAppear(bool animated) {
            base.ViewWillAppear(animated);
            SimpleEventBus.Subscribe<PopoverMenuToggleRequested>(OnPopoverMenuToggled);

            if (null == _dataMap) {
                var emptyState = new UIImageView(Theme.DetailEmptyStateImage) {
                    ContentMode = UIViewContentMode.TopLeft
                };

                View = emptyState;
            }
        }

        public override void ViewWillDisappear(bool animated) {
            base.ViewWillDisappear(animated);
            SimpleEventBus.Unsubscribe<PopoverMenuToggleRequested>(OnPopoverMenuToggled);
        }


        public UIViewController SelectedController {
            get { return _selectedController; }
        }
    }
}
