using System;
using System.Collections.Generic;

namespace softwrench.sW4.Shared2.Metadata.Applications.Command {
    public class ApplicationCommand {

        private readonly string _id;
        private readonly string _label;

        public string Service { get; set; }
        public string Icon { get; set; }
        public string Method { get; set; }

        public string DefaultPosition { get; set; }

        public List<string> ScopeParameters { get; set; }

        private readonly bool _remove;
        private readonly string _role;
        private readonly string _showExpression;
        private readonly string _successMessage;
        private readonly string _nextSchemaId;
        

        private readonly ApplicationCommandStereotype _stereotype;

        public ApplicationCommand(string id, string label, string service, string method, bool remove, string role, string stereotype, string showExpression, string successMessage, string nextSchemaId, string scopeParameters, string defaultPosition, string icon) {
            _id = id;
            _label = label;
            Service = service;
            _remove = remove;
            _role = role;
            Method = method;
            if (!String.IsNullOrEmpty(stereotype)) {
                Enum.TryParse(stereotype, true, out _stereotype);
            }
            _showExpression = showExpression;
            _successMessage = successMessage;
            _nextSchemaId = nextSchemaId;
            if (scopeParameters != null) {
                ScopeParameters = new List<string>(scopeParameters.Split(','));
            }
            DefaultPosition = defaultPosition;
            Icon = icon;
        }

        public string Id {
            get { return _id; }
        }

        public string Label {
            get { return _label; }
        }



        public bool Remove {
            get { return _remove; }
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
            return string.Format("Id: {0}, Label: {1}, Stereotype: {2}, Remove: {3}, Service: {4},Method: {5} ", _id, _label, _stereotype, _remove, Service, Method);
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