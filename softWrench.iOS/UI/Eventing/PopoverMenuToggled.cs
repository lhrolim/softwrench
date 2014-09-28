namespace softWrench.iOS.UI.Eventing
{
    public sealed class PopoverMenuToggled
    {
        private readonly bool _show;

        public PopoverMenuToggled(bool show)
        {
            _show = show;
        }

        public bool Show
        {
            get
            {
                return _show;
            }
        }
    }
}