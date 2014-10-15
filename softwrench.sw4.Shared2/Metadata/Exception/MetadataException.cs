using System;

namespace softwrench.sw4.Shared2.Metadata.Exception {
    public class MetadataException : InvalidOperationException {


        public MetadataException(string message) : base(message) { }

        public static MetadataException MisingIdStereotypeInCommand() {
            throw new MetadataException("Command must declare either Id or a Stereotype");
        }

        public static MetadataException CommandBarNotFound(string commandBarId) {
            throw new MetadataException(String.Format("Command bar {0} not found, review your metadata configuration", commandBarId));
        }

        public static System.Exception CommandNotFound(string commanddisplayableId,string commandBarId)
        {
            throw new MetadataException(String.Format("Command {0} not found in commandbar {1}. review your metadata configuration", commanddisplayableId, commandBarId));
        }
    }
}
