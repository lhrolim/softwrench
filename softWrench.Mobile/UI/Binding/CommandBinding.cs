using softWrench.Mobile.Behaviors;

namespace softWrench.Mobile.UI.Binding
{
    internal class CommandBinding
    {
        private readonly IApplicationCommand _command;

        public CommandBinding(IApplicationCommand command)
        {
            _command = command;
        }

        /// <summary>
        ///     Evaluates whether the command is currently
        ///     available for the given context.
        /// </summary>
        /// <param name="formBinding">The form binding where the command is registered.</param>
        public bool IsAvailable(FormBinding formBinding)
        {
            if (false == Command.IsAvailableOnNew && formBinding.IsNew)
            {
                return false;
            }

            if (false == Command.IsAvailableOnLocal && formBinding.DataMap.LocalState.IsLocal)
            {
                return false;
            }

            return Command.IsAvailable(formBinding.DataMap);
        }

        public IApplicationCommand Command
        {
            get { return _command; }
        }
    }
}