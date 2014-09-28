using System;
using softWrench.Mobile.Data;

namespace softWrench.Mobile.Behaviors
{
    internal interface IApplicationCommand : IDisposable
    {
        /// <summary>
        ///     Executes the command using the specified arguments.
        /// </summary>
        /// <param name="arguments">The command arguments.</param>
        void Execute(IApplicationCommandArguments arguments);
        
        /// <summary>
        ///     Allows the command to opt-in to be displayed on the screen
        ///     using <seealso cref="OnBeforeShowContext.RegisterCommand"/>
        ///     If it resigns to do so, the command will not be available.
        /// </summary>
        /// <param name="context">The registration context.</param>
        /// <param name="dataMap">The data map about to be shown.</param>
        void Register(OnBeforeShowContext context, DataMap dataMap);

        /// <summary>
        ///     Evaluates whether the command is currently available
        ///     for interaction in the given data map state.
        /// </summary>
        /// <param name="dataMap">The current state of the data map.</param>
        /// <remarks>
        ///     The command has a chance to evaluate its availability
        ///     only if the current state is compatible with settings
        ///     <seealso cref="IsAvailableOnNew"/> and <seealso cref="IsAvailableOnLocal"/>.
        ///     Otherwise the command is forcibly unavailable.
        /// </remarks>
        bool IsAvailable(DataMap dataMap);

        string Label { get; }        
        string Name { get; }
        string Subtitle { get; }
        string Title { get; }
        bool IsAvailableOnNew { get; }
        bool IsAvailableOnLocal { get; }
    }
}