using System.Collections.Generic;
using Newtonsoft.Json;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Util;
using System.Xml.Serialization;
using System;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.UI;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema {
    public class ApplicationSection : IApplicationAttributeDisplayable, IApplicationDisplayableContainer, IPCLCloneable {

        private const string WrongRenderer = "orientation {0} not found. Possible options are horizontal and vertical";

        public string Id { get; set; }
        public string ApplicationName { get; set; }
        public bool Abstract { get; set; }
        public string Resourcepath { get; set; }
        public string Label { get; set; }
        public string Attribute { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
        public string ShowExpression { get; set; }
        public string EnableExpression { get; set; }
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

        public string Qualifier { get; set; }

        public FieldRenderer Renderer {
            get { return _renderer; }
            set { _renderer = value; }
        }



        public ApplicationSection() {

        }

        public ApplicationSection(string id, string applicationName,
            bool @abstract, string label, string attribute, string resourcepath,
            string parameters, List<IApplicationDisplayable> displayables, string showExpression,
            string toolTip, string orientation, ApplicationHeader header, FieldRenderer renderer) {
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

        public string RendererType { get { return null; } }
        public IDictionary<string, string> RendererParameters { get { return Renderer.ParametersAsDictionary(); } }
        public string Type { get { return GetType().Name; } }
        public string Orientation { get { return OrientationEnum.ToString().ToLower(); } }
        public string Role { get { return ApplicationName + "." + Id; } }

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
            resultDisplayables, ShowExpression, ToolTip, Orientation, Header, _renderer);
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
