using cts.commons.simpleinjector.Events;

namespace softwrench.sw4.user.classes.services.setup {
    public class UserSavedEvent : ISWEvent {
        public bool AllowMultiThreading { get { return false; } }

        public UserSavedEvent(int userId, string login, string maximoPersonId, bool isCreation) {
            Login = login;
            UserId = userId;
            MaximoPersonId = maximoPersonId;
            IsCreation = isCreation;
        }

        public string MaximoPersonId { get; }
        public bool IsCreation { get; set; }

        public string Login;
        public int UserId;


    }
}
