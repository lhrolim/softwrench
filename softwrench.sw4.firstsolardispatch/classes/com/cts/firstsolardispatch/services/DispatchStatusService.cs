using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softwrench.sW4.audit.Interfaces;
using static softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model.DispatchTicketStatus;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services {
    public class DispatchStatusService : ISingletonComponent {

        [Import]
        public IAuditManager AuditManager { get; set; }

        public void ValidateStatusChange(DispatchTicketStatus old, DispatchTicketStatus newone, DispatchTicket ticket, bool acceptSame = true) {
            if (newone == old) {
                if (acceptSame) return;
                StatusException(old, newone);
            }
            if (old == DRAFT && (newone == SCHEDULED || newone == DISPATCHED)) return;
            if (old == SCHEDULED && newone == DISPATCHED) return;
            if (old == DISPATCHED) {
                if (newone == ACCEPTED) return;
                if (newone == REJECTED) {
                    if (ticket.DispatchExpectedDate == null) return;
                    var diff = DateTime.Now - ticket.DispatchExpectedDate.Value;
                    // less than 4 hours
                    if (diff.TotalMilliseconds < 4 * 1000 * 3600) {
                        return;
                    }
                    StatusException(old, newone, " It is not possible to reject a ticket after four hours after dispatched.");
                }
            }
            if (old == ACCEPTED && newone == ARRIVED) return;
            if (old == ARRIVED && newone == RESOLVED) return;
            StatusException(old, newone);
        }

        private void StatusException(DispatchTicketStatus old, DispatchTicketStatus newone, string reason = "") {
            throw new Exception($"Is not possible to change the ticket from status \"{old.LabelName()}\" to status \"{newone.LabelName()}\".{reason}");
        }
    }
}
