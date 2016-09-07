namespace cts.commons.simpleinjector.Events {
    public class ContainerReloadedEvent : ISWEvent {
        public bool AllowMultiThreading { get { return true; } }
    }
}
