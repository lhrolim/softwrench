using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using cts.commons.Util;

namespace softwrench.sw4.api.classes.email {

    public class EmailAttachment {
        public EmailAttachment(string attachmentData, string attachmentName) {
            AttachmentData = attachmentData;
            AttachmentName = attachmentName;
        }

        public string AttachmentData { get; set; }
        public string AttachmentName { get; set; }
    }
}
