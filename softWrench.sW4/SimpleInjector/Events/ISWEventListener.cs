namespace softWrench.sW4.SimpleInjector.Events {
    public interface ISWEventListener<in T> : IComponent {
        void HandleEvent(T eventToDispatch);
    }
}