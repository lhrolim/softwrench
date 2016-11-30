using System;

namespace softwrench.sw4.user.classes.exceptions {
    public class UserActivationException : Exception {

        public UserActivationException(string message) : base(message) {

        }

        public static UserActivationException NoSecurityGroups(string username) {
            return new UserActivationException($"User {username} has no permissions set. Assign at least one security group to him");
        }

      

    }
}
