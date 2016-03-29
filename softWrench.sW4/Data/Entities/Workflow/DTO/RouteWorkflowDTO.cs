using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.Entities.Workflow.DTO {
    public class InitWorkflowDTO {

        public string OwnerId {
            get; set;
        }
        public string OwnerTable {
            get; set;
        }
        public string AppUserId {
            get; set;
        }
        public string SiteId {
            get; set;
        }
        public string WfId {
            get; set;
        }
        public string ProcessName {
            get; set;
        }
        public string Memo {
            get; set;
        }
        public string ActionId {
            get; set;
        }
        public string AssignmentId {
            get; set;
        }

    }
}
