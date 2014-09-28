using System;
using MonoTouch.UIKit;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.iOS.Controllers
{
	public partial class DetailComponentNavigationController : UINavigationController
	{
        private ApplicationSchemaDefinition _applicationMetadata;
        private DataMap _dataMap;
        private bool _isNew;
	    private CompositeDataMap _composite;
	    private DetailController _detailController;
	    private Action<DetailComponentController.Result> _onCompletion;

	    public DetailComponentNavigationController (IntPtr handle) : base (handle)
		{
		}

        public void Construct(ApplicationSchemaDefinition application, DataMap dataMap, bool isNew, CompositeDataMap composite, DetailController detailController, Action<DetailComponentController.Result> onCompletion)
        {
            _applicationMetadata = application;
            _dataMap = dataMap;
            _isNew = isNew;
            _composite = composite;
            _detailController = detailController;
            _onCompletion = onCompletion;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var controller = Storyboard.InstantiateViewController<DetailComponentController>();
            controller.Construct(_applicationMetadata, _dataMap, _isNew, _composite, _detailController, _onCompletion);
            PushViewController(controller, false);
        }

        public override bool DisablesAutomaticKeyboardDismissal
        {
            // Avoids the keyboard to stubbornly refuse to close
            // itself when displayed on a modal form sheet.
            // http://stackoverflow.com/questions/3372333/ipad-keyboard-will-not-dismiss-if-modal-view-controller-presentation-style-is-ui
            get { return true; }
        }
	}
}
