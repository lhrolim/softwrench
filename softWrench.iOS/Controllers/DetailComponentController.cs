using System;
using System.Threading.Tasks;
using MonoTouch.UIKit;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Applications;

using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.iOS.Controllers
{
	public partial class DetailComponentController : UIViewController
	{
        private ApplicationSchemaDefinition _applicationMetadata;
        private DataMap _dataMap;
	    private bool _isNew;
	    private Action<Result> _onCompletion;
	    private DetailFormController _formController;

	    public DetailComponentController (IntPtr handle) : base (handle)
		{
        }

        public void Construct(ApplicationSchemaDefinition application, DataMap dataMap, bool isNew, CompositeDataMap composite, DetailController detailController, Action<Result> onCompletion)
        {
            _applicationMetadata = application;
            _dataMap = dataMap;
            _onCompletion = onCompletion;
            _isNew = isNew;

            // Creates the controller that
            // contains the actual form.
            _formController = Storyboard.InstantiateViewController<DetailFormController>();
            _formController.Construct(_applicationMetadata, _dataMap, isNew, composite, detailController, OnCommand, t => Title = t);
            AddChildViewController(_formController);
            _formController.DidMoveToParentViewController(this);
        }

	    private Task<bool> OnCommand()
	    {
            //TODO: commands in the composite (i.e. parent) data map
            //      always save data before execution. What are the
            //      implications in a component (i.e. child) data map?
	        throw new NotImplementedException();
	    }

	    private void SubscribeToBus()
        {

        }

        private void UnsubscribeFromBus()
        {
        }

        private void Show()
        {
            _formController
                .View
                .Frame = containerView.Bounds;

            // Performs the view transfer dance.
            containerView.AddSubview(_formController.View);
            _formController.View.MovedToSuperview();
        }

	    private void OnCompletion(bool isSuccess)
	    {
            var onCompletion = _onCompletion;

            DismissViewController(true, null);

            if (null == onCompletion)
            {
                return;
            }

            onCompletion(new Result(_dataMap, isSuccess, _isNew));

            // We don't want to keep unnecessary references
            // to (possibly) other controllers.
            _onCompletion = null;
	    }
        
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        partial void Done(UIBarButtonItem sender)
        {           
            using (ActivityIndicator.Start(View))
            {
                if (false == _formController.Commit())
                {
                    return;
                }

                // TODO: we're not firing OnSave in the child
                // controller because the changes will only
                // hit the database when the parent controller
                // decides to do it.
                // _formController.OnSave();

                OnCompletion(true);
            }
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        partial void Cancel(UIBarButtonItem sender)
        {            
            OnCompletion(false);
        }

	    protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeFromBus();
            }
                        
            // Releases the completion handler to
            // avoid hanging other controllers.
            _onCompletion = null;

            base.Dispose(disposing);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (null == _applicationMetadata)
            {
                return;
            }

            // If the data map already exists on the
            // server, we won't allow its edition.
            if (false == _dataMap.LocalState.IsLocal)
            {
                NavigationItem.SetRightBarButtonItems(new UIBarButtonItem[0], false);
            }

            SubscribeToBus();
            Show();
        }

	    public class Result
	    {
	        private readonly DataMap _dataMap;
	        private readonly bool _isSuccess;
	        private readonly bool _isNew;

	        public Result(DataMap dataMap, bool isSuccess, bool isNew)
	        {
	            _dataMap = dataMap;
	            _isSuccess = isSuccess;
	            _isNew = isNew;
	        }

	        public DataMap DataMap
	        {
	            get { return _dataMap; }
	        }

	        public bool IsSuccess
	        {
	            get { return _isSuccess; }
	        }

	        public bool IsNew
	        {
	            get { return _isNew; }
	        }
	    }
	}
}
