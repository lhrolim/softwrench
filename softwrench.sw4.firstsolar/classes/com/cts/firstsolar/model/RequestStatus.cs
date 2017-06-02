using NHibernate.Type;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {

    public enum RequestStatus {
        Scheduled, Sent, Approved, Rejected, Pending, Error
    }

    public class RequestStatusConverter : EnumStringType<RequestStatus> {
    }


    public static class RequestStatusExtensions {

        public static bool IsSubmitted(this RequestStatus status) {
            return !status.Equals(RequestStatus.Scheduled);
        }

        public static string LabelName(this RequestStatus status) {
            if (status == RequestStatus.Approved) {
                return "approval";
            }

            if (status == RequestStatus.Rejected) {
                return "rejection";
            }

            if (status == RequestStatus.Pending) {
                return "pending status";
            }

            return status.ToString();
        }


    }

}
