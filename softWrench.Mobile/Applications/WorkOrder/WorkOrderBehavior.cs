using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.Data;
using softWrench.Mobile.Persistence;
using softWrench.Mobile.Persistence.Expressions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Applications.WorkOrder {
    internal class WorkOrderBehavior : ApplicationBehavior {
        public const string Class = "WORKORDER";


        public WorkOrderBehavior(IEnumerable<IApplicationCommand> commands)
            : base(commands) {
        }

        public override void OnNew(OnNewContext context, DataMap dataMap) {
            base.OnNew(context, dataMap);

            var sequence = context
                .MetadataRepository
                .LoadAndIncrementSequenceAsync(context.Application, "wonum")
                .Result;

            var statusField = context.Application.Fields.First(f => f.Attribute == "status");
            if (statusField.DefaultValue == null) {
                statusField.DefaultValue = "WAPPR";
            }

            //TODO: review hardcoded values
            // Upon adding new field initializers, consider
            // also changing the work order follow-up command.
            dataMap.Value("wonum", sequence.Format());
            dataMap.Value("woclass", Class);
            dataMap.Value("status", statusField.DefaultValue);
            dataMap.Value("synstatus_.maxvalue", "WAPPR");
            dataMap.Value("synstatus_.description", statusField.DefaultValue == "OPEN" ? "Work Request Open" : "Waiting on Approval");
            dataMap.Value("reportdate", DateTime.Now);
            dataMap.Value("changeby", context.User.UserName);
        }

        public override void OnBeforeSave(OnBeforeSaveContext context, DataMap dataMap) {
            base.OnBeforeSave(context, dataMap);

            dataMap.Value("changeby", context.User.UserName);
        }

        public override void OnBeforeUpload(OnBeforeUploadContext context, DataMap dataMap) {
            base.OnBeforeUpload(context, dataMap);

            // TODO: solve the conundrum of not
            // being able to send the wo status
            // when uploading existing orders.
            if (false == dataMap.LocalState.IsLocal) {
                context.Content.Remove("status");
            }
        }
    }
}
