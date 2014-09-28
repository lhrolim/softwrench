namespace softWrench.sW4.SimpleInjector.Events
{
    /// <summary>
    /// Base event interface for events dispatched inside sw application
    /// </summary>
    public interface ISWEvent 
    {
        bool AllowMultiThreading { get; }
    }
}