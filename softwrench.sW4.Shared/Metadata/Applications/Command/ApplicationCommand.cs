using System;

namespace softwrench.sW4.Shared.Metadata.Applications.Command {
    public class ApplicationCommand {

        private readonly string _id;
        private readonly string _label;
        private readonly string _clientresourcepath;
        private readonly string _clientFunction;
        private readonly bool _remove;
        private readonly string _role;
        private readonly string _showExpression;
        private readonly string _successMessage;
        private readonly string _nextSchemaId;

        private readonly ApplicationCommandStereotype _stereotype;

        public ApplicationCommand(string id, string label, string clientresourcepath, string clientFunction, bool remove, string role, string stereotype, string showExpression, string successMessage, string nextSchemaId) {
            _id = id;
            _label = label;
            _clientresourcepath = clientresourcepath;
            _remove = remove;
            _role = role;
            _clientFunction = clientFunction;
            if (!String.IsNullOrEmpty(stereotype)) {
                Enum.TryParse(stereotype, true, out _stereotype);
            }
            _showExpression = showExpression;
            _successMessage = successMessage;
            _nextSchemaId = nextSchemaId;
        }

        public string Id {
            get { return _id; }
        }

        public string Label {
            get { return _label; }
        }

        public string Clientresourcepath {
            get { return _clientresourcepath; }
        }

        public bool Remove {
            get { return _remove; }
        }

        public string ClientFunction {
            get { return _clientFunction; }
        }

        public string Role {
            get { return _role; }
        }

        public string ShowExpression {
            get { return _showExpression; }
        }

        public string SuccessMessage {
            get { return _successMessage; }
        }

        public string NextSchemaId {
            get { return _nextSchemaId; }
        }

        public ApplicationCommandStereotype Stereotype {
            get { return _stereotype; }
        }

        public override string ToString() {
            return string.Format("Id: {0}, Label: {1}, Stereotype: {2}, Remove: {3}, Clientresourcepath: {4}", _id, _label, _stereotype, _remove, _clientresourcepath);
        }

        protected bool Equals(ApplicationCommand other) {
            return string.Equals(_id, other._id);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationCommand)obj);
        }

        public override int GetHashCode() {
            return (_id != null ? _id.GetHashCode() : 0);
        }
    }
}