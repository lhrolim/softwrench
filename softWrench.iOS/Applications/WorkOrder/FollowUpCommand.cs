using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using softWrench.iOS.Behaviors;
using softWrench.iOS.UI.Eventing;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Metadata.Parsing;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.iOS.Applications.WorkOrder {
    internal class FollowUpCommand : IApplicationCommand {
        private static void CopyValue(DataMap to, DataMap from, string attribute) {
            to.Value(attribute, @from.Value(attribute));
        }

        private static void ConfigureFollowUp(DataMap originator, DataMap followUp, Sequence sequence, User user, ApplicationSchemaDefinition definition) {
            var statusField = definition.Fields.First(f => f.Attribute == "status");
            
            // The follow-up work order should not be sent
            // to the server as the usual CRUD change. This
            // is because sending a follow up to the server
            // requires special processing, which will be
            // taken care by a specific data operation handler.
            followUp
                .LocalState
                .SetFlag(LocalStateFlag.SkipCrudSynchronization);

            if (statusField.DefaultValue == null) {
                statusField.DefaultValue = "WAPPR";
            }

            //TODO: review values
            //TODO: ensure OnNew does not change this
            //TODO: this logic should be shared with other mobile platforms
            followUp.Value("wonum", sequence.Format());
            followUp.Value("status", statusField.DefaultValue);
            followUp.Value("synstatus_.maxvalue", "WAPPR");
            followUp.Value("synstatus_.description", statusField.DefaultValue == "OPEN" ? "Work Request Open" : "Waiting on Approval");
            followUp.Value("reportdate", DateTime.Now);
            followUp.Value("changeby", user.UserName);

            // These properties are specific
            // to the follow-up operation.
            followUp.Value("origrecordid", originator.Value("wonum"));
            followUp.Value("origrecordclass", originator.Value("woclass"));

            CopyValue(followUp, originator, "orgid");
            CopyValue(followUp, originator, "siteid");
            CopyValue(followUp, originator, "woclass");
            CopyValue(followUp, originator, "location");
            CopyValue(followUp, originator, "location_.description");
            CopyValue(followUp, originator, "owner");
            CopyValue(followUp, originator, "wopriority");
            CopyValue(followUp, originator, "assetnum");
            CopyValue(followUp, originator, "asset_.description");
            CopyValue(followUp, originator, "failurecode");
            CopyValue(followUp, originator, "failurecode_.description");
            CopyValue(followUp, originator, "worktype");
            CopyValue(followUp, originator, "worktype_.wtypedesc");
            CopyValue(followUp, originator, "glaccount");
            CopyValue(followUp, originator, "chartofaccounts_.accountname");
        }

        private static void ConfigureOriginator(DataMap originator, User user) {
            originator.Value("changeby", user.UserName);
            originator.Value("hasfollowupwork", 1);
        }

        private static DataOperation CreateDataOperation(IApplicationCommandArguments args, DataMap followUp) {
           
            var data = new Dictionary<string, string>
                       {
                           {"localid", followUp.LocalState.LocalId.ToString()},
                           {"origrecordid", followUp.Value("origrecordid")},
                           {"origrecordclass", followUp.Value("origrecordclass")}
                       };

            return new DataOperation(args.ApplicationSchemaDefinition, data, typeof(Handler));
        }

        public async void Execute(IApplicationCommandArguments arguments) {
            var args = (ApplicationCommandArguments)arguments;

            // TODO: Execute the OnNew pipeline?
            var sequence = await args
                .MetadataRepository
                .LoadAndIncrementSequenceAsync(args.ApplicationSchemaDefinition, "wonum");

            var originator = args.DataMap;
            var followUp = new DataMap(args.ApplicationSchemaDefinition);

            ConfigureFollowUp(originator, followUp, sequence, args.User, arguments.ApplicationSchemaDefinition);
            ConfigureOriginator(originator, args.User);

            // Save the originator work order with our recent
            // modifications. We can safely bypass the Save
            // method provided by the controller because by
            // design the data is saved before the command
            // execution, so we just need to save our changes.
            await args
                .DataRepository
                .SaveAsync(args.ApplicationSchemaDefinition, originator);

            // And now let's save a data operation
            // to register it and later process it.
            await args
                .DataRepository
                .SaveAsync(CreateDataOperation(args, followUp));

            Alert.Show("Follow-Up", "Here is your new work order. You can now provide the follow-up details.", explicitlyInvokeOnMainThread: true);

            SimpleEventBus.Publish(new DataMapSaved(originator));
            SimpleEventBus.Publish(new DataMapSelected(followUp, true));
        }

        /// <seealso cref="IApplicationCommand.Register"/>
        public void Register(OnBeforeShowContext context, DataMap dataMap) {
            context.RegisterCommand(this);
        }

        /// <seealso cref="IApplicationCommand.IsAvailable"/>
        public bool IsAvailable(DataMap dataMap) {
            return true;
        }

        public string Name {
            get { return GetType().FullName; }
        }

        public string Label {
            get { return "Follow-Up"; }
        }

        public string Subtitle {
            get { return "A new work order will be created based on this one."; }
        }

        public string Title {
            get { return "Create a Follow-up"; }
        }

        public bool IsAvailableOnNew {
            get { return false; }
        }

        public bool IsAvailableOnLocal {
            get { return false; }
        }

        private sealed class Handler : IDataOperationHandler {
            private DataMap _followUp;

            public async Task<string> HandleAsync(DataOperation dataOperation, DataRepository repository) {
                var localId = Guid.Parse(dataOperation.Data["localid"]);

                _followUp = await repository
                    .PeekAsync(localId);

                // If we couldn't find the follow-up
                // it's because it was deleted. Let's
                // abort the handling then.
                if (null == _followUp) {
                    return null;
                }

                var data = new {
                    origrecordid = dataOperation.Data["origrecordid"],
                    origrecordclass = dataOperation.Data["origrecordclass"],
                    crud = _followUp.Fields
                };

                return JsonConvert.SerializeObject(data, JsonParser.SerializerSettings);
            }

            public async Task Success(SynchronizationRepository repository) {
                await repository
                    .MarkCompositeDataMapAsSynchronizedAsync(_followUp);
            }

            public string Name {
                get { return "CreateFollowUp"; }
            }
        }

        public void Dispose() {
        }
    }
}