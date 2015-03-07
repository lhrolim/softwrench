namespace cts.commons.simpleinjector.Events {
    public class ApplicationStartedEvent : ISWEvent {
        public bool AllowMultiThreading { get { return true; } }
    }
}
