namespace cts.commons.simpleinjector.Events {
    public interface IEventDispatcher : ISingletonComponent {
        void Dispatch<T>(T eventToDispatch) where T : class;


    }
}