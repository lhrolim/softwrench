using System;

namespace softWrench.sW4.Exceptions {
    public class UnauthorizedException : Exception {
        public UnauthorizedException() : base() { }

        public UnauthorizedException(string msg) : base(msg) { }

        /// <param name="username"></param>
        /// <returns>UnauthorizedException with appropriate message 
        /// for the case where the given username is not authenticated</returns>
        public static UnauthorizedException NotAuthenticated(string username = null) {
            var msg = string.IsNullOrEmpty(username) ? "No user authenticated." : "User " + username + "  not authenticated.";
            return new UnauthorizedException(msg);
        }

        /// <param name="username"></param>
        /// <returns>UnauthorizedException with appropriate message 
        /// for the case where the given username is not authenticated</returns>
        public static UnauthorizedException UserNotFound(string username) {
            return new UnauthorizedException("User " + username + " not registered.");
        }

    }
}
