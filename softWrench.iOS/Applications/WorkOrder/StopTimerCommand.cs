using System;
using System.Threading.Tasks;
using softWrench.iOS.Behaviors;
using softWrench.iOS.Controllers;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Parsing;

namespace softWrench.iOS.Applications.WorkOrder
{
    internal class StopTimerCommand : IApplicationCommand
    {
        // TODO: How we should reference the LABORTRANS
        // application? By name, entity, guid... ?
        private const string LaborApplicationName = "labtrans";

        /// <summary>
        ///     Creates a new labtrans data map using
        ///     the specified start and finish date as
        ///     the default values.
        /// </summary>
        /// <param name="arguments">The application command arguments.</param>
        /// <param name="utcStart">The labor UTC start time.</param>
        /// <param name="utcFinish">The labor UTC finish time.</param>
        private static async Task<DataMap> CreateLabor(IApplicationCommandArguments arguments, DateTime utcStart, DateTime utcFinish)
        {
            var laborMetadata = await arguments
                .MetadataRepository
                .LoadApplicationAsync(LaborApplicationName);

            var labor = await arguments
                .DataRepository
                .NewAsync(laborMetadata, arguments.Composite);
            
            var regularHours = Convert.ToInt32((utcFinish - utcStart).TotalHours);

            labor.Value("startdate", utcStart.ToLocalTime());
            labor.Value("starttime", utcStart.ToLocalTime());
            labor.Value("finishdate", utcFinish.ToLocalTime());
            labor.Value("finishtime", utcFinish.ToLocalTime());
            labor.Value("regularhrs", regularHours);

            return labor;
        }

        /// <seealso cref="IApplicationCommand.Execute"/>
        public async void Execute(IApplicationCommandArguments arguments)
        {
            var args = (ApplicationCommandArguments) arguments;

            // Let's deserialize the timer data
            // stored by the start button.
            var timerData = JsonParser.FromJson<TimerData>(args
                .DataMap
                .CustomFields[TimerData.CustomFieldKey]);

            // And now we can create the new labor.
            var labor = await CreateLabor(args, timerData.UtcStart, DateTime.UtcNow);

            // We don't need the timer data anymore.
            args.DataMap
                .CustomFields
                .Remove(TimerData.CustomFieldKey);

            // As we've emptied timer data, now we
            // need to save the parent data map.
            await ((DetailController) args
                .Controller)
                .SaveAsync();

            // Finally, let's instruct the controller to
            // activate and display the new labor on the
            // screen so the user can complement it and
            // commit the operation.
            ((DetailController) args
                .Controller)
                .ShowSegment(LaborApplicationName, labor);
        }

        /// <seealso cref="IApplicationCommand.Register"/>
        public void Register(OnBeforeShowContext context, DataMap dataMap)
        {
            if (StartTimerCommand.IsTrackable(dataMap))
            {
                context.RegisterCommand(this);
            }
        }

        /// <seealso cref="IApplicationCommand.IsAvailable"/>
        public bool IsAvailable(DataMap dataMap)
        {
            // Let's make sure there is indeed a
            // timer in progress for this data map.
            return dataMap
                .CustomFields
                .ContainsKey(TimerData.CustomFieldKey);
        }

        public string Name
        {
            get { return GetType().FullName; }
        }

        public string Label
        {
            get { return "Stop Timer"; }
        }

        public string Subtitle
        {
            get { return "Stops the timer and pre-fills it as a labor."; }
        }

        public string Title
        {
            get { return "Stop Timer"; }
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