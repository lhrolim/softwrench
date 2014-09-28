using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using softWrench.iOS.Behaviors;
using softWrench.iOS.Controllers;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Parsing;
using softWrench.Mobile.Persistence;
using softWrench.Mobile.Persistence.Expressions;
using softWrench.Mobile.Utilities;

namespace softWrench.iOS.Applications.WorkOrder {
    internal class ChangeStatusCommand : IApplicationCommand {
        private static string NormalizeStatus(DataMap dataMap) {
            var maxstatus = dataMap.Value("synstatus_.maxvalue");

            // We'll perform a case-insensitive
            // comparison in the Maximo status
            // (as the synonym is customizable).
            return null != maxstatus
                ? maxstatus.ToUpperInvariant()
                : null;
        }

        private static bool CanChangeStatus(DataMap dataMap) {
            switch (NormalizeStatus(dataMap)) {
                case "CLOSE":
                case "CAN":
                    return false;

                default:
                    return true;
            }
        }

        private static DataOperation CreateDataOperation(ApplicationCommandArguments arguments) {
            // TODO: use UTC?
            var statusDate = DataMap.ConvertType(DateTime.Now);

            var dataMap = arguments.DataMap;
            var data = new Dictionary<string, string>
                       {
                           {"localid", dataMap.LocalState.LocalId.ToString()},
                           {"wonum", dataMap.Value("wonum")},
                           {"status", dataMap.Value("status")},
                           {"statusdate", statusDate}
                       };

            return new DataOperation(arguments.ApplicationSchemaDefinition, data, typeof(Handler));
        }

        private static IEnumerable<FilterExpression> CreateFilters(DataMap dataMap) {
            const string inexistentStatus = "+$+inexistent-status+$+";

            switch (NormalizeStatus(dataMap)) {

                case "COMP":

                    // An OPEN work order can only be CLOSED.
                    return new List<FilterExpression>
                           {
                               new Exactly("maxvalue", "CLOSE")
                           };

                case "CLOSE":
                case "CAN":

                    // CLOSED and CANCELED are final.
                    // TODO: While we don't have filtering
                    // operand and operators, let's play
                    // with our luck a little.
                    return new List<FilterExpression>
                           {
                               new Exactly("maxvalue", inexistentStatus)
                           };

                default:
                    return Enumerable.Empty<FilterExpression>();
            }
        }

        private static async void OnLookupCompleted(ApplicationCommandArguments commandArguments) {
            // We'll ask the controller to save the changes
            // instead of doing ourselves because it will
            // continue active in the screen, and we don't
            // want to mess with its state.
            var success = await ((DetailController)commandArguments
                .Controller)
                .SaveAsync();

            if (false == success) {
                return;
            }

            // Registers the status change
            // in our operation ledger.
            await commandArguments
                .DataRepository
                .SaveAsync(CreateDataOperation(commandArguments));
        }

        public void Execute(IApplicationCommandArguments arguments) {
            var args = (ApplicationCommandArguments)arguments;
            var detailController = (DetailController)args.Controller;
            var formController = (DetailFormController)detailController.SelectedController;

            var statusField = args
                .ApplicationSchemaDefinition
                .Fields
                .First(f => f.Attribute == "synstatus_.description");

            // We'll need to know when the status lookup
            // completes so we can actually perform the
            // status change operation.
            var onCompletion = new Action<LookupController.Result>(result => OnLookupCompleted(args));

            // And now tells the controller to trigger the
            // navigation to the work order status lookup.
            var adHocFilters = CreateFilters(args.DataMap);
            detailController.InvokeOnMainThread(() => formController.Lookup(statusField, onCompletion, adHocFilters));
        }

        /// <seealso cref="IApplicationCommand.Register"/>
        public void Register(OnBeforeShowContext context, DataMap dataMap) {
            if (CanChangeStatus(dataMap)) {
                context.RegisterCommand(this);
            }
        }

        /// <seealso cref="IApplicationCommand.IsAvailable"/>
        public bool IsAvailable(DataMap dataMap) {
            return true;
        }

        public string Name {
            get { return GetType().FullName; }
        }

        public string Label {
            get { return "Change Status"; }
        }

        public string Subtitle {
            get { return "Changes the status of this work order."; }
        }

        public string Title {
            get { return "Change Status"; }
        }

        public bool IsAvailableOnNew {
            get { return false; }
        }

        public bool IsAvailableOnLocal {
            get { return false; }
        }

        private sealed class Handler : IDataOperationHandler {
            private static string Handle(DataOperation dataOperation) {
                var data = new {
                    wonum = dataOperation.Data["wonum"],
                    status = dataOperation.Data["status"],
                    statusdate = dataOperation.Data["statusdate"]
                };

                return JsonConvert.SerializeObject(data, JsonParser.SerializerSettings);
            }

            public Task<string> HandleAsync(DataOperation dataOperation, DataRepository repository) {
                return Task
                    .Factory
                    .StartNew(() => Handle(dataOperation));
            }

            public Task Success(SynchronizationRepository repository) {
                return CompletedTask.Instance;
            }

            public string Name {
                get { return "UpdateStatus"; }
            }
        }

        public void Dispose() {
        }
    }
}