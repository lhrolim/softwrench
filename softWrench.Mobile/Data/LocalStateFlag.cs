using System;

namespace softWrench.Mobile.Data
{
    [Flags]
    public enum LocalStateFlag
    {
        /// <summary>
        ///     No special behavior.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Changes performed on the data map of CRUD nature (e.g.
        ///     changing fields through the UI) must not be sent to the
        ///     server. A <see cref="IDataOperationHandler"/> or other
        ///     specific behavior will be responsible for processing it.
        /// </summary>
        SkipCrudSynchronization = 2 ^ 0
    }
}