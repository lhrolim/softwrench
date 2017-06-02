using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FSWPackageConstants {
        public const string WorklogsRelationship = "wkpgworklogs_";
        public const string AttachsRelationship = "wkpgattachments_";
        public const string CallOutAttachsRelationship = "wkpgcoattachments_";


//        public class CallOutStatus {
//            public const string Open = "Open";
//            public const string Completed = "Completed";
//            public const string SubmitAfterSave = "Submit After Save";
//            public const string Submited = "Submited";
//        }

        public class MaintenanceEngStatus {
            public const string Pending = "Pending";
            public const string SubmitAfterSave = "Submit After Save";
            public const string Submited = "Submited";
            public const string Accepted = "Accepted";
            public const string Rejected = "Rejected";

            public static readonly IList<string> SubmitedStatus = new ReadOnlyCollection<string>(new List<string>() {
                Submited,
                Accepted,
                Rejected
            });
        }
    }
}
