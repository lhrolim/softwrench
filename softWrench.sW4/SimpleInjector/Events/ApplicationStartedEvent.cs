namespace softWrench.sW4.SimpleInjector.Events {
    public class ApplicationStartedEvent : ISWEvent {
        public bool AllowMultiThreading { get { return true; } }
    }
}
