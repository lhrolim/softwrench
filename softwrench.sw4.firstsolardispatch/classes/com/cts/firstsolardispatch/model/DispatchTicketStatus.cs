namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model {
    public enum DispatchTicketStatus {
        DRAFT, SCHEDULED, DISPATCHED, ACCEPTED, REJECTED, ARRIVED, RESOLVED
    }

    public static class DispatchTicketStatusExtensions {
        public static string LabelName(this DispatchTicketStatus status) {
            if (status == DispatchTicketStatus.DRAFT) {
                return "Draft";
            }
            if (status == DispatchTicketStatus.SCHEDULED) {
                return "Scheduled";
            }
            if (status == DispatchTicketStatus.DISPATCHED) {
                return "Dispatched";
            }
            if (status == DispatchTicketStatus.ACCEPTED) {
                return "Accepted";
            }
            if (status == DispatchTicketStatus.REJECTED) {
                return "Rejected";
            }
            if (status == DispatchTicketStatus.ARRIVED) {
                return "Arrived";
            }
            if (status == DispatchTicketStatus.RESOLVED) {
                return "Resolved";
            }
            return status.ToString();
        }


    }
}