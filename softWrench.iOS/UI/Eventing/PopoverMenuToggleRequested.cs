namespace softWrench.iOS.UI.Eventing {
    public sealed class PopoverMenuToggleRequested {
        private readonly bool _show;

        public PopoverMenuToggleRequested(bool show) {
            _show = show;
        }

        public bool Show {
            get {
                return _show;
            }
        }
    }
}