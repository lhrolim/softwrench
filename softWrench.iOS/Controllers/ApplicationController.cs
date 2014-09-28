using System;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using softWrench.iOS.UI.Eventing;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.iOS.Controllers
{
	public partial class ApplicationController : BaseController
    {
	    private static void RegisterTransitionFadeAnimation(UIViewController subNavigationController)
	    {
	        Theme.Fade(subNavigationController.View);
	    }

	    private const float MasterWidth = 321;
	    private readonly WeakReference<DetailController> _activeDetailController;
	    private ApplicationSchemaDefinition _applicationMetadata;
	    private bool _wasLandscape = true;
	    private bool _isMasterPopoverShown;

	    public ApplicationController (IntPtr handle) : base (handle)
		{
            _activeDetailController = new WeakReference<DetailController>(null);
		}

	    public void Construct(ApplicationSchemaDefinition schema)
		{
            _applicationMetadata = schema;
//            Title = _applicationMetadata.Title;
		}

	    private void SubscribeToBus()
        {
            SimpleEventBus.Subscribe<DataMapSelected>(OnDataMapSelected);
            SimpleEventBus.Subscribe<PopoverMenuToggleRequested>(OnPopoverMenuToggleRequested);
        }

	    private void UnsubscribeFromBus()
        {
            SimpleEventBus.Unsubscribe<DataMapSelected>(OnDataMapSelected);
            SimpleEventBus.Unsubscribe<PopoverMenuToggleRequested>(OnPopoverMenuToggleRequested);
        }

	    private void ShowPopover()
        {
            if (!_isMasterPopoverShown)
            {
                AnimateMasterView(true);
                SimpleEventBus.Publish(new PopoverMenuToggled(true));
            }
        }

	    private void HidePopover()
        {
            if (_isMasterPopoverShown)
            {
                AnimateMasterView(false);
                SimpleEventBus.Publish(new PopoverMenuToggled(false));
            }
        }

	    private void AnimateMasterView(bool visible)
        {
            UIView.BeginAnimations("SwitchOrientation");
            UIView.SetAnimationDuration(.3);
            UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);

            var frame = masterView.Frame;
            frame.X = visible ? 0 : -MasterWidth;
            masterView.Frame = frame;

            UIView.CommitAnimations();
            _isMasterPopoverShown = visible;
        }

	    private void SwitchOrientation(UIInterfaceOrientation orientation, bool animated, double duration = .5)
        {
            if (orientation.IsLandscape())
            {
                if (_wasLandscape)
                {
                    return;
                }

                if (_isMasterPopoverShown)
                {
                    AnimateMasterView(false);
                }

                if (animated)
                {
                    UIView.BeginAnimations("SwitchOrientation");
                    UIView.SetAnimationDuration(duration);
                    UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);
                }

                //Slide the masterView inward
                var frame = masterView.Frame;
                frame.X = 0;
                masterView.Frame = frame;

                //Shrink the detailView
                frame = detailView.Frame;
                frame.X += MasterWidth;
                frame.Width -= MasterWidth;
                detailView.Frame = frame;

                if (animated)
                {
                    UIView.CommitAnimations();
                }
                _wasLandscape = true;
            }
            else
            {
                if (!_wasLandscape)
                {
                    return;
                }

                if (animated)
                {
                    UIView.BeginAnimations("SwitchOrientation");
                    UIView.SetAnimationDuration(duration);
                    UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);
                }

                //Slide the masterView off screen
                var frame = masterView.Frame;
                frame.X = -frame.Width;
                masterView.Frame = frame;

                //Grow the detailView
                frame = detailView.Frame;
                frame.X -= MasterWidth;
                frame.Width += MasterWidth;
                detailView.Frame = frame;

                if (animated)
                {
                    UIView.CommitAnimations();
                }
                _wasLandscape = false;
            }
        }

	    private void ReplaceActiveDetailController(DetailController controller)
	    {
	        DetailController previousController;

            // Not sure why, but MonoTouch returns true
            // after SetTarget(null). Maybe some boxing?
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
	        if (_activeDetailController.TryGetTarget(out previousController) && null != previousController)
	        {
	            previousController.Dispose();
	        }

            _activeDetailController.SetTarget(controller);
	    }

	    private async void NavigateToDetail(DataMapSelected e)
	    {
	        HidePopover();

            var compositeDataMap = await ExpandCompositeDataMap(e.DataMap);

	        // TODO: make less brittle
	        var subNavigationController = ChildViewControllers[0].ChildViewControllers[0].NavigationController;

	        // Let's fade the detail page when
	        // a new item is selected.
	        RegisterTransitionFadeAnimation(subNavigationController);

	        // How about a brand-new detail controller
	        // to display the selected data map?
	        var detailController = Storyboard.InstantiateViewController<DetailController>();
            detailController.Construct(compositeDataMap, e.IsNew);
	        subNavigationController.SetViewControllers(new UIViewController[] {detailController}, false);

	        // We need to dispose the previous active
	        // controller and set this one as the new one.
	        ReplaceActiveDetailController(detailController);
	    }

	    private Task<CompositeDataMap> ExpandCompositeDataMap(DataMap dataMap)
	    {
	        return CompositeDataMap.Expand(_applicationMetadata, dataMap);
	    }

	    private void OnDataMapSelected(DataMapSelected e)
        {
            InvokeOnMainThread(() => NavigateToDetail(e));
        }

	    private void OnPopoverMenuToggleRequested(PopoverMenuToggleRequested e)
        {
            if (false == e.Show)
            {
                HidePopover();
                return;
            }

            ShowPopover();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeFromBus();
            }

            base.Dispose(disposing);
        }   

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            SwitchOrientation(InterfaceOrientation, false);

            // TODO: make less brittle;
            var list = (ListController) ChildViewControllers[1].ChildViewControllers[0];
            list.Construct(_applicationMetadata);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            SubscribeToBus();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            UnsubscribeFromBus();
        }

        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            base.WillRotate(toInterfaceOrientation, duration);
            SwitchOrientation(toInterfaceOrientation, true, duration);
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            if (_isMasterPopoverShown && evt.TouchesForView(masterView) == null)
            {
                HidePopover();
            }
        }
	}
}
