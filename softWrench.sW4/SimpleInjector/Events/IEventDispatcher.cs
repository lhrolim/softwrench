namespace softWrench.sW4.SimpleInjector.Events {
    public interface IEventDispatcher : ISingletonComponent {
        void Dispatch<T>(T eventToDispatch) where T : class;


    }
}