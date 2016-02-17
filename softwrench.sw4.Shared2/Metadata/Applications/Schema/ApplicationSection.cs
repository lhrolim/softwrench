using System.Collections.Generic;
using cts.commons.portable.Util;
using Newtonsoft.Json;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Metadata.Applications.UI;
using softwrench.sW4.Shared2.Util;
using System;
using System.ComponentModel;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema {
    public class ApplicationSection : IApplicationAttributeDisplayable, IApplicationDisplayableContainer, IPCLCloneable {

        private const string WrongRenderer = "orientation {0} not found. Possible options are horizontal and vertical";

        public string Id { get; set; }
        public string ApplicationName { get; set; }
        public bool Abstract { get; set; }
        public string Resourcepath { get; set; }
        public string Label { get; set; }
        public string Attribute { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        [DefaultValue("true")] public string ShowExpression { get; set; }
        public string ToolTip { get; set; }
        public bool? ReadOnly {
            get { return false; }
            set { }
        }

        public List<IApplicationDisplayable> _displayables = new List<IApplicationDisplayable>();
        public ApplicationSectionOrientation OrientationEnum { get; set; }
        public ApplicationHeader Header { get; set; }

        [JsonIgnore]
        public ApplicationSchemaDefinition.LazyComponentDisplayableResolver ComponentDisplayableResolver;

        private Boolean AreComponentsResolved = false;
        private string _parametersString;
        private FieldRenderer _renderer;
        private string _role;
        public string RendererType {
            get { return _renderer.RendererType; }
        }

        public IDictionary<string, object> RendererParameters {
            get { return _renderer == null ? new Dictionary<string, object>() : _renderer.ParametersAsDictionary(); }
        }

        public FieldRenderer Renderer {
            get { return _renderer; }
            set { _renderer = value; }
        }
        public string Qualifier { get; set; }

        public bool SecondaryContent { get; set; }

        public bool IsHidden {
            get; set;
        }

        public ApplicationSection() {

        }

        public ApplicationSection(string id, string applicationName,
            bool @abstract, string label, string attribute, string resourcepath,
            string parameters, List<IApplicationDisplayable> displayables, string showExpression,
            string toolTip, string orientation, ApplicationHeader header, FieldRenderer renderer,
            string role) {
            Id = id;
            ApplicationName = applicationName;
            Abstract = @abstract;
            Resourcepath = resourcepath;
            _parametersString = parameters;
            Parameters = PropertyUtil.ConvertToDictionary(parameters);
            _displayables = displayables;
            Label = label;
            Attribute = attribute;
            ShowExpression = showExpression;
            ToolTip = toolTip;
            ValidateOrientation(orientation);
            Header = header;
            _renderer = renderer;
            Role = role;
            }
        protected virtual void ValidateOrientation(string orientation) {

            ApplicationSectionOrientation result;

            if (!String.IsNullOrWhiteSpace(orientation)) {
                if (!Enum.TryParse(orientation, true, out result)) {
                    throw new InvalidOperationException(String.Format(WrongRenderer, orientation));
                }
                OrientationEnum = result;
            } else {
                OrientationEnum = ApplicationSectionOrientation.vertical;
            }
        }

        public string Type { get { return GetType().Name; } }
        public string Orientation { get { return OrientationEnum.ToString().ToLower(); } }

        public string Role
        {
            get { return _role ?? ApplicationName + "." + Id; }
            set { _role = value; }
        }

        public override string ToString() {
            return string.Format("Id: {0}, Displayables: {1}, Abstract: {2}", Id, Displayables.Count, Abstract);
        }

        public object Clone() {
            var resultDisplayables = new List<IApplicationDisplayable>();

            foreach (var applicationDisplayable in Displayables) {
                var cloneable = applicationDisplayable as IPCLCloneable;
                if (cloneable != null) {
                    resultDisplayables.Add((IApplicationDisplayable)cloneable.Clone());
                } else {
                    resultDisplayables.Add(applicationDisplayable);
                }
            }
            return new ApplicationSection(Id, ApplicationName, Abstract, Label, Attribute, Resourcepath, _parametersString,
            resultDisplayables, ShowExpression, ToolTip, Orientation, Header, _renderer, Role) {
                SecondaryContent = SecondaryContent
            };
        }

        public List<IApplicationDisplayable> Displayables {
            get {

                if (!AreComponentsResolved) {

                }
                return _displayables;
            }
            set { _displayables = value; }
        }
    }
}
