using System;

namespace softWrench.Mobile.Data
{
    /// <summary>
    ///     Contains data and behavior configurations for
    ///     data maps that are intended to be used by the
    ///     client platform.
    /// </summary>
    public sealed class LocalState
    {
        /// <summary>
        ///     Sets the specified flag in the
        ///     <see cref="Flags"/> property.
        /// </summary>
        /// <param name="flag">The flag to set.</param>
        public void SetFlag(LocalStateFlag flag)
        {
            Flags = Flags | flag;
        }

        /// <summary>
        ///     Determines whether one or more flag are
        ///     set in the <see cref="Flags"/> property.
        /// </summary>
        /// <param name="flag">The flag (or flags) to check.</param>
        public bool HasFlag(LocalStateFlag flag)
        {
            return Flags.HasFlag(flag);
        }

        /// <summary>
        ///     Gets or sets the unique identifier of the
        ///     data map in the local (client) database.
        /// </summary>
        public Guid LocalId { get; set; }

        /// <summary>
        ///     Gets or sets the unique identifier of the data
        ///     map in the local (client) database that is the
        ///     parent of the current data map. Only applicable
        ///     for component (i.e. "child") data maps that are
        ///     part of composition relationships.
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        ///     Gets or sets whether the data map exists only
        ///     locally, i.e., was created on the client and
        ///     has not been uploaded to the remote server yet.
        /// </summary>
        public bool IsLocal { get; set; }

        /// <summary>
        ///     Gets or sets if the synchronization with the
        ///     remote server failed and thus there are still
        ///     pending changes stored locally.
        /// </summary>
        public bool IsBouncing { get; set; }

        /// <summary>
        ///     For bouncing data (i.e. data facing sync errors
        ///     with the remote server) returns the description
        ///     of the error returned by the server. Otherwise,
        ///     returns <see langword="null"/>.
        /// </summary>
        public string BounceReason { get; set; }

        /// <summary>
        ///     Gets or sets a mask of flags that influence
        ///     the behavior applied on the data map.
        /// </summary>
        public LocalStateFlag Flags { get; set; }
    }
}