using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using cts.commons.Util;

namespace softwrench.sw4.api.classes.email {
    public class EmailData {

        public EmailData(string sendFrom, string sendTo, string subject, string message, List<EmailAttachment> attachments = null) {
            Validate.NotNull(sendTo, "sentTo");
            Validate.NotNull(subject, "Subject");
            SendFrom = sendFrom;
            SendTo = sendTo;
            Subject = subject;
            Message = message;
            Attachments = attachments;
        }

        public string SendFrom { get; set; }
        public string SendTo { get; set; }
        public string Cc { get; set; }
        public string BCc { get; set; }
        public string Subject { get; set; }
        public List<EmailAttachment> Attachments { get; set; }
        public string Message { get; set; }
    }
}
