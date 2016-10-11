namespace cts.commons.simpleinjector.Events {
    public interface IEventDispatcher : ISingletonComponent {
        void Dispatch<T>(T eventToDispatch, bool parallel = false) where T : class;
        void Fire<T>(T eventToDispatch, bool parallel = false) where T : class;
    }
}