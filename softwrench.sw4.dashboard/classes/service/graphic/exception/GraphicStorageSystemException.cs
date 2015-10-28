using System;

namespace softwrench.sw4.dashboard.classes.service.graphic.exception {
    public class GraphicStorageSystemException : Exception {
        public GraphicStorageSystemException(string message) : base(message) { }

        public GraphicStorageSystemException(string message, Exception cause) : base(message, cause) { }

        public static GraphicStorageSystemException ServiceNotFound(string systemName, Exception cause = null) {
            var message = string.Format("No service found that supports the graphic storage system '{0}'", systemName);
            return Instance(message, cause);
        }

        public static GraphicStorageSystemException AuthenticationFailed(string systemName, string url, Exception cause = null) {
            var message = string.Format("Failed to authenticate to graphic storage system '{0}' at '{1}'", systemName, url);
            return Instance(message, cause);
        }

        private static GraphicStorageSystemException Instance(string message, Exception cause = null) {
            return cause != null ? new GraphicStorageSystemException(message, cause) : new GraphicStorageSystemException(message);
        }
    }
}
