using cts.commons.simpleinjector.Events;

namespace softWrench.sW4.Security {
    public class UserSavedEvent : ISWEvent {
        public bool AllowMultiThreading { get { return false; } }

        public UserSavedEvent(int userId, string login) {
            Login = login;
            UserId = userId;
        }

        public string Login;
        public int UserId;


    }
}
