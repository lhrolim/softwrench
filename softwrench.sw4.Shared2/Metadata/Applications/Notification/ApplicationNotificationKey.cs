using System;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared2.Metadata.Applications.Notification {

    /// <summary>
    /// The key which fully indentifies a notification to be renderized to the clients.
    /// </summary>
    public class ApplicationNotificationKey : ApplicationMetadataSchemaKey {

        public const string NotFoundPattern = "Notification of {0} type for role {1} not found";

        private NotificationType _type;
        private string _role;
        private string _notificationId;
        
        public InvalidOperationException NotFoundException() {
            return new InvalidOperationException(String.Format(NotFoundPattern, _type, _role));
        }

        public ApplicationNotificationKey() { }

        public ApplicationNotificationKey(string notificationId, NotificationType type, string role) {
            _type = type;
            _role = role;
        }

        public NotificationType Type {
            get { return _type; }
            set { _type = value; }
        }

        public string Role {
            get { return _role; }
            set { _role = value; }
        }

        public string NotificationId {
            get { return _notificationId; }
            set { _notificationId = value; }
        }

        protected bool Equals(ApplicationNotificationKey other) {
            return string.Equals(_type, other._type) && string.Equals(_role, other._role) && string.Equals(_notificationId, other.NotificationId);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationNotificationKey)obj);
        }

        public override string ToString() {
            return string.Format("NotificationId: {0}, Type: {1}, Role: {2}", _notificationId, _type, _role);
        }

    }
}
