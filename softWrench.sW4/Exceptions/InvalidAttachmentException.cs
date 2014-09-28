using System;

namespace softWrench.sW4.Exceptions {
    public class InvalidAttachmentException : InvalidOperationException {


        public InvalidAttachmentException(string message) : base(message) { }

        public static InvalidAttachmentException BlankFileNotAllowed() {
            throw new InvalidOperationException("Blank files are not allowed");
        }
    }
}
