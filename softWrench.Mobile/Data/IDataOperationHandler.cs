using System.Threading.Tasks;
using softWrench.Mobile.Persistence;

namespace softWrench.Mobile.Data
{
    internal interface IDataOperationHandler
    {
        /// <summary>
        ///     Process the data operation and returns the
        ///     Json that must be sent to the server.
        /// </summary>
        /// <param name="dataOperation">The data operation to handle.</param>
        /// <param name="repository">The repository in use by the context.</param>
        Task<string> HandleAsync(DataOperation dataOperation, DataRepository repository);

        /// <summary>
        ///     Notifies the handler that the data operation
        ///     was successfully sent to the remote server.
        /// </summary>
        /// <param name="repository">The synchronization repository in use by the context.</param>
        Task Success(SynchronizationRepository repository);

        /// <summary>
        ///     Gets the name of the operation as
        ///     expected by the remote server.
        /// </summary>
        string Name { get; }
    }
}
