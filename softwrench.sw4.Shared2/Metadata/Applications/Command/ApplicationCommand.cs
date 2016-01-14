using System;
using System.Collections.Generic;
using cts.commons.portable.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using softwrench.sw4.Shared2.Metadata.Applications.Command;

namespace softwrench.sW4.Shared2.Metadata.Applications.Command {
    public class ApplicationCommand : ICommandDisplayable {

        private readonly string _id;

        public string Service {
            get; set;
        }
        public string Icon {
            get; set;
        }
        public string Tooltip {
            get; set;
        }
        public string Method {
            get; set;
        }

        public string CssClasses {
            get; set;
        }
        public bool? Primary {
            get; set;
        }

        public string Position {
            get; set;
        }
        public string Type {
            get {
                return typeof(ApplicationCommand).Name;
            }
        }

        public ICommandDisplayable KeepingOriginalData(ICommandDisplayable originalCommand)
        {
            var originalCommandCasted =(ApplicationCommand) originalCommand;
            Icon = Icon ?? originalCommandCasted.Icon;
            Label = Label ?? originalCommandCasted.Label;
            Tooltip = Tooltip ?? originalCommandCasted.Tooltip;
            Primary = Primary ?? originalCommandCasted.Primary;
            ShowExpression = ShowExpression ?? originalCommandCasted.ShowExpression;
            EnableExpression = EnableExpression ?? originalCommandCasted.EnableExpression;
            return this;
        }

        public List<string> ScopeParameters {
            get; set;
        }

        public IDictionary<string, object> Parameters {
            get; set;
        }
        public IDictionary<string, object> Properties {
            get; set;
        }

        private readonly string _role;
        private readonly string _successMessage;
        private readonly string _nextSchemaId;


        private readonly ApplicationCommandStereotype _stereotype;

        public ApplicationCommand(string id, string label, string service, string method, string role, string stereotype, string showExpression, string enableExpression, string successMessage,
            string nextSchemaId, string scopeParameters, string properties, string defaultPosition, string icon, string tooltip, string cssClasses, bool primary) {
            _id = id;
            Label = label;
            Service = service;
            _role = role;
            Method = method;
            if (!String.IsNullOrEmpty(stereotype)) {
                Enum.TryParse(stereotype, true, out _stereotype);
            }
            ShowExpression = showExpression;
            EnableExpression = enableExpression ?? "true";
            _successMessage = successMessage;
            _nextSchemaId = nextSchemaId;
            if (scopeParameters != null) {
                ScopeParameters = new List<string>(scopeParameters.Split(','));
                Parameters = PropertyUtil.ConvertToDictionary(scopeParameters, false);
            }
            if (properties != null) {
                Properties = PropertyUtil.ConvertToDictionary(properties, false);
            }
            Position = defaultPosition;
            Icon = icon;
            Tooltip = tooltip;
            CssClasses = cssClasses;
            Primary = primary;
        }

        public string Id {
            get {
                return _id;
            }
        }

        public string Label { get; set; }





        public string Role {
            get {
                return _role;
            }
        }

        public string ShowExpression { get; set; }

        public string EnableExpression {
            get; set;
        }

        public string SuccessMessage {
            get {
                return _successMessage;
            }
        }

        public string NextSchemaId {
            get {
                return _nextSchemaId;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public ApplicationCommandStereotype Stereotype {
            get {
                return _stereotype;
            }
        }

        public override string ToString() {
            return string.Format("Id: {0}, Label: {1}, Stereotype: {2}, Service: {3},Method: {4} ", _id, Label, _stereotype, Service, Method);
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

        public static ApplicationCommand TestInstance(String id, string position = "", string label = null, string icon = null) {
            return new ApplicationCommand(id, label, null, null, null, null, null, null, null, null, null, null, position, icon, null, null,false);
        }
    }
}