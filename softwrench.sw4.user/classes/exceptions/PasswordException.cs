using System;

namespace softwrench.sw4.user.classes.exceptions {
    public class PasswordException : Exception {
        public enum Type {
            History, Locked
        }

        public Type ExceptionType {
            get; set;
        }

        public PasswordException(string message, Type type) : base(message) {
            ExceptionType = type;
        }

        public class PasswordHistoryException : PasswordException {
            public PasswordHistoryException(string message, Type type) : base(message, type) {
            }
        }

        public static PasswordException HistoryException(int threshold) {
            return new PasswordHistoryException($"password cannot be any of the the last {threshold} stored passwords", Type.History);
        }

        public static PasswordException LockedException(string username) {
            return new PasswordException($"user ${username} is locked. Please contact your administrator", Type.Locked);
        }

    }
}
