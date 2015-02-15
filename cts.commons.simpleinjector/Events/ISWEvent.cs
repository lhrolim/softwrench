namespace cts.commons.simpleinjector.Events {
    /// <summary>
    /// Base event interface for events dispatched inside sw application
    /// </summary>
    public interface ISWEvent {
        bool AllowMultiThreading { get; }
    }
}