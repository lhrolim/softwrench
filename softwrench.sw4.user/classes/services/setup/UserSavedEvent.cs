using cts.commons.simpleinjector.Events;

namespace softwrench.sw4.user.classes.services.setup {
    public class UserSavedEvent : ISWEvent {
        public bool AllowMultiThreading { get { return false; } }

        public UserSavedEvent(int userId, string login, bool isCreation) {
            Login = login;
            UserId = userId;
            IsCreation = isCreation;
        }

        public bool IsCreation { get; set; }

        public string Login;
        public int UserId;


    }
}
