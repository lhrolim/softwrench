using System;
using cts.commons.portable.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.callout.exception {

    public class IFSEmailWorkflowException : Exception {


        public IFSEmailWorkflowException(string message) : base(message) {

        }

        public static IFSEmailWorkflowException NotFound<T>() where T : IFsEmailRequest, new() {
            var type = new T();
            return new IFSEmailWorkflowException("{0} not found. Please contact support".Fmt(type.EntityDescription));
        }

        public static IFSEmailWorkflowException AlreadyApprovedRejected<T>(int? calloutId, RequestStatus status) where T : IFsEmailRequest, new() {

            var text = "approved";
            if (status.Equals(RequestStatus.Rejected)) {
                text = "rejected";
            }

            return new IFSEmailWorkflowException("Callout {0} is already {1}".Fmt(new T().EntityDescription, calloutId, text));
        }
    }
}
