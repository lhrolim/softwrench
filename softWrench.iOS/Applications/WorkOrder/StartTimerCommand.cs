using System;
using softWrench.iOS.Behaviors;
using softWrench.iOS.Controllers;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Parsing;

namespace softWrench.iOS.Applications.WorkOrder
{
    internal class StartTimerCommand : IApplicationCommand
    {
        private static string NormalizeStatus(DataMap dataMap)
        {
            var maxstatus = dataMap.Value("synstatus_.maxvalue");

            // We'll perform a case-insensitive
            // comparison in the Maximo status
            // (as the synonym is customizable).
            return null != maxstatus
                ? maxstatus.ToUpperInvariant()
                : null;
        }

        /// <summary>
        ///     Evaluates whether the work order can have a timer
        ///     manipulated (started or stopped) for job tracking
        ///     purposes. It checks only if the work order itself
        ///     is in a (Maximo) workflow state that would allow
        ///     labor tracking, it does NOT take into account if 
        ///     there is already a timer in progress.
        /// </summary>
        /// <param name="dataMap">The work order.</param>s
        public static bool IsTrackable(DataMap dataMap)
        {
            switch (NormalizeStatus(dataMap))
            {
                case "APPR":
                case "COMP":
                case "INPRG":
                case "WMATL":
                case "WSCH":
                    return true;

                default:
                    return false;
            }
        }

        /// <seealso cref="IApplicationCommand.Execute"/>
        public async void Execute(IApplicationCommandArguments arguments)
        {
            var args = (ApplicationCommandArguments) arguments;

            // Stores a custom field tracking
            // which time the timer started.
            args.DataMap
                .CustomFields[TimerData.CustomFieldKey] = new TimerData(DateTime.UtcNow).ToJson();

            // We'll ask the controller to save the changes
            // instead of doing ourselves because it will
            // continue active in the screen, and we don't
            // want to mess with its state.
            var success = await ((DetailController) args
                .Controller)
                .SaveAsync();

            if (success)
            {
                Alert.Show("Timer Started", "The app is now tracking the time spent on this work order.", explicitlyInvokeOnMainThread: true);                
            }
        }

        /// <seealso cref="IApplicationCommand.Register"/>
        public void Register(OnBeforeShowContext context, DataMap dataMap)
        {
            // Does this work order satisfies the
            // minimum requirements to be tracked?
            // (e.g. currently in progress, ...)
            if (IsTrackable(dataMap))
            {
                context.RegisterCommand(this);
            }
        }

        /// <seealso cref="IApplicationCommand.IsAvailable"/>
        public bool IsAvailable(DataMap dataMap)
        {
            // Let's make sure there is not another
            // timer in progress for this data map.
            return false == dataMap
                .CustomFields
                .ContainsKey(TimerData.CustomFieldKey);
        }

        public string Name
        {
            get { return GetType().FullName; }
        }

        public string Label
        {
            get { return "Start Timer"; }
        }

        public string Subtitle
        {
            get { return "Starts tracking the time spent on this work order."; }
        }

        public string Title
        {
            get { return "Start Timer"; }
        }

        public bool IsAvailableOnNew
        {
            get { return false; }
        }

        public bool IsAvailableOnLocal
        {
            get { return false; }
        }

        public void Dispose()
        {
        }
    }
}