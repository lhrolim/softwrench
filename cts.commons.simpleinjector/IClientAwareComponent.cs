namespace cts.commons.simpleinjector
{
    public interface IClientAwareComponent : ISingletonComponent
    {

        /// <summary>
        /// Comma separated list of clients that implementations of this interface should refer to. If null, any Client would accept it.
        /// </summary>
        /// <returns></returns>
        string ClientFilters();
    }
}