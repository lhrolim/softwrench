using System;
using cts.commons.persistence;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {
    public interface IFsEmailRequest : IBaseEntity {

        string Token { get; set; }
        string Notes { get; set; }
        string EntityDescription { get; }

        string ByToken { get; }

        RequestStatus? Status { get; set; }

        WorkPackage WorkPackage { get; set; }
        string Email { get; set; }
        string Cc { get; set; }
        DateTime? ActualSendTime { get; set; }
    }
}