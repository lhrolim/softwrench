using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {

    public class WorkPackageEmailRequestAdapter : IFsEmailRequest {

        public WorkPackageEmailRequestAdapter(WorkPackage workPackage) {
            Token = workPackage.AccessToken;
            EntityDescription = "Work Package";
            WorkPackageId = workPackage.Id.Value;
        }

        public string Token { get; set; }
        public string Notes { get; set; }
        public string EntityDescription { get; }

        public string ByToken => "from WorkPackage where accesstoken = ?";

        public RequestStatus Status { get; set; }
        public int? Id { get; set; }
        public int WorkPackageId { get; set; }
        public string Email { get; set; }
        public string Cc { get; set; }
        public DateTime? ActualSendTime { get; set; }
    }
}
