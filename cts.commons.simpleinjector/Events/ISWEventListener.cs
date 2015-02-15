namespace cts.commons.simpleinjector.Events {
    public interface ISWEventListener<in T> : IComponent {
        void HandleEvent(T eventToDispatch);
    }
}