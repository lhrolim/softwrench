using System;

namespace softWrench.sW4.Exceptions {
    public class MetadataException : InvalidOperationException {


        public MetadataException(string message) : base(message) { }

        public static MetadataException MisingIdStereotypeInCommand() {
            throw new MetadataException("Command must declare either Id or a Stereotype");
        }
    }
}
