using System;
using System.Threading.Tasks;
using MonoTouch.UIKit;
using softWrench.iOS.Behaviors;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.UI.Binding;

namespace softWrench.iOS.Views
{
	public partial class DetailCommandCell : UITableViewCell
	{
        private const float CellHeight = 88;

        internal static float GetHeightForRow(CommandBinding binding)
        {
            return CellHeight;
        }

	    private IApplicationCommand _command;
	    private ApplicationCommandArguments _arguments;
	    private Func<Task<bool>> _preCondition;

	    public DetailCommandCell (IntPtr handle) : base (handle)
		{
		}

        internal void Construct(IApplicationCommand command, ApplicationCommandArguments arguments, Func<Task<bool>> preCondition)
        {
            _command = command;
            _preCondition = preCondition;
            _arguments = arguments;

            title.Text = command.Title;
            subtitle.Text = command.Subtitle;
            button.SetTitle(command.Label, UIControlState.Normal);

            subtitle.SizeToFit();
            button.SetBackgroundImage(Theme.CommandButton, UIControlState.Normal);
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
	    async partial void ButtonTouchUpInside(UIButton sender)
	    {
            // Starts evaluating the pre-condition.
	        var isPreConditionSatisfied = await _preCondition();

	        // We'll only execute the command if
            // the pre-condition holds.
	        if (isPreConditionSatisfied)
	        {
	            _command.Execute(_arguments);
	        }
	    }        
	}
}
