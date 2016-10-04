using softwrench.sw4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using softwrench.sW4.Shared2.Metadata.Applications.UI;
using System;
using System.Collections.Generic;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema {

    public class ApplicationFieldDefinition : BaseApplicationFieldDefinition, IDefinition, IPCLCloneable {

        public const string AttributeQualifierSeparator = ".";

        private FieldRenderer _renderer;
        private FieldFilter _filter;
        private IWidgetDefinition _widgetDefinition;

        private ISet<ApplicationEvent> _eventsSet = new HashSet<ApplicationEvent>();
        public string EvalExpression { get; set; }

        public ApplicationFieldDefinition() {

        }

        public ApplicationFieldDefinition(string applicationName, string attribute, string label, string requiredExpression, bool isReadOnly, bool isIsHidden,
             FieldRenderer renderer, FieldFilter filter, IWidgetDefinition widgetDefinition, string defaultValue, string qualifier, string showExpression, string toolTip,
             string attributeToServer, ISet<ApplicationEvent> events, string enableExpression, string evalExpression)
            : base(applicationName, label, attribute, requiredExpression, isReadOnly, defaultValue, qualifier, showExpression, toolTip, attributeToServer, events, enableExpression) {
            if (widgetDefinition == null) throw new ArgumentNullException("widgetDefinition");
            _widgetDefinition = widgetDefinition;
            _renderer = renderer;
            _filter = filter;
            IsHidden = isIsHidden;
            if (renderer == null || String.IsNullOrEmpty(renderer.RendererType)) {
                var newRenderer = BuildFromWidget();
                if (newRenderer != null) {
                    _renderer = newRenderer;
                }
            }
            _eventsSet = events;
            EvalExpression = evalExpression;
        }
        //TODO: choose one of the modes?
        private FieldRenderer BuildFromWidget() {
            if (_widgetDefinition is DateWidgetDefinition) {
                var dateWidget = ((DateWidgetDefinition)_widgetDefinition);
                return new FieldRenderer(FieldRenderer.BaseRendererType.DATETIME.ToString().ToLower(), String.Format("time={0};format={1}", dateWidget.Time, dateWidget.Format), Attribute);
            } else if (_widgetDefinition is NumberWidgetDefinition) {
                var numberWidget = ((NumberWidgetDefinition)_widgetDefinition);
                return new FieldRenderer(FieldRenderer.BaseRendererType.NUMERICINPUT.ToString().ToLower(), String.Format("min={0};max={1};decimals={2}", numberWidget.Min, numberWidget.Max, numberWidget.Decimals), Attribute);
            }
            return null;
        }


        public bool IsHidden { get; set; }

        public FieldRenderer Renderer {
            get { return _renderer; }
            set { _renderer = value; }
        }

        public FieldFilter Filter {
            get { return _filter; }
            set { _filter = value; }
        }

        public IWidgetDefinition WidgetDefinition {
            get { return _widgetDefinition; }
            set { _widgetDefinition = value; }
        }


        public bool IsAssociated {
            get {
                return Attribute.Contains(AttributeQualifierSeparator);
            }
        }

        public override string RendererType {
            get { return _renderer.RendererType; }
        }

        public override IDictionary<string, string> RendererParameters {
            get { return _renderer == null ? new Dictionary<string, string>() : _renderer.ParametersAsDictionary(); }
        }
        public IDictionary<string, string> FilterParameters {
            get { return _filter == null ? new Dictionary<string, string>() : _filter.ParametersAsDictionary(); }
        }

        protected bool Equals(ApplicationFieldDefinition other) {
            return string.Equals(Attribute, other.Attribute);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ApplicationFieldDefinition)obj);
        }

        public override int GetHashCode() {
            return (Attribute != null ? Attribute.GetHashCode() : 0);
        }

        public override string ToString() {
            return Tuple.Create("attr:", Attribute, "label", Label).ToString();
        }

        public object Clone() {
            return new ApplicationFieldDefinition(ApplicationName, Attribute, Label, RequiredExpression, IsReadOnly, IsHidden,
                Renderer, Filter, WidgetDefinition, DefaultValue, Qualifier, ShowExpression, ToolTip, AttributeToServer, _eventsSet, EnableExpression,EvalExpression);
        }

        public static ApplicationFieldDefinition HiddenInstance(string applicationName, string attributeName) {
            return new ApplicationFieldDefinition(applicationName, attributeName, "", "false", false, true,
                        new FieldRenderer(), new FieldFilter(), new HiddenWidgetDefinition(), null, null, null, null, null, null, null,null);
        }
    }
}