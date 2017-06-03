using System;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {
    public interface IFsEmailRequest {

        string Token { get; set; }
        string Notes { get; set; }
        string EntityDescription { get; }

        string ByToken { get; }

        RequestStatus Status { get; set; }

        int? Id { get; set; }
        int WorkPackageId { get; set; }
        string Email { get; set; }
        DateTime? ActualSendTime { get; set; }
    }
}