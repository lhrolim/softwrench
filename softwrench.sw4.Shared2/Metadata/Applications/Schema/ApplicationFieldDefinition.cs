using softwrench.sw4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using softwrench.sW4.Shared2.Metadata.Applications.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema {

    public class ApplicationFieldDefinition : BaseApplicationFieldDefinition, IPCLCloneable {

        public const string AttributeQualifierSeparator = ".";

        private FieldRenderer _renderer;
        private FieldFilter _filter;

        //TODO: remove this, since it�s Xamarin legacy code
        [Obsolete("Only used by unsupported Xamarin client code")]
        private IWidgetDefinition _widgetDefinition;

        private readonly ISet<ApplicationEvent> _eventsSet = new HashSet<ApplicationEvent>();
        public string EvalExpression {
            get; set;
        }
        [DefaultValue("true")]
        public string EnableDefault {
            get; set;
        }

        /// <summary>
        /// whether this field has been auto generated by the fwk rather than declared on the medatada (such as some pks and fks)
        /// </summary>
        public bool AutoGenerated {
            get; set;
        }

        public ApplicationFieldDefinition() {

        }

        public ApplicationFieldDefinition(string applicationName, string attributeName, string datatype, String label, string requiredExpression, Boolean isReadOnly, bool isHidden, FieldRenderer renderer
            , IWidgetDefinition widget, string defaultValue, string tooltip, bool fromSubquery)
            : this(applicationName, attributeName, datatype, label, requiredExpression, isReadOnly, isHidden, renderer, null, widget, defaultValue, null, null, tooltip, null, null, null, null, null, null, fromSubquery, null, false) {
        }

        public ApplicationFieldDefinition(string applicationName, string attributeName, String label) {
            ApplicationName = applicationName;
            Attribute = attributeName;
            Label = label;
        }

        public ApplicationFieldDefinition(string applicationName, string attribute, string datatype, string label, string requiredExpression, bool isReadOnly, bool isIsHidden,
             FieldRenderer renderer, FieldFilter filter, IWidgetDefinition widgetDefinition, string defaultValue, string qualifier, string showExpression, string toolTip,
             string attributeToServer, ISet<ApplicationEvent> events, string enableExpression, string evalExpression, string enableDefault, string defaultExpression, bool declaredAsQueryOnEntity,
             string noResultsTarget, bool preventNoresultsCarry)
            : base(applicationName, label, attribute, requiredExpression, isReadOnly, defaultValue, qualifier, showExpression, toolTip, attributeToServer, events, enableExpression, defaultExpression, declaredAsQueryOnEntity) {
            if (widgetDefinition == null) throw new ArgumentNullException("widgetDefinition");
            _widgetDefinition = widgetDefinition;
            _renderer = renderer;
            _filter = filter;
            DataType = datatype;
            IsHidden = isIsHidden;
            if (renderer == null || String.IsNullOrEmpty(renderer.RendererType)) {
                var newRenderer = BuildFromWidget();
                if (newRenderer != null) {
                    _renderer = newRenderer;
                }
            }
            _eventsSet = events;
            EvalExpression = evalExpression;
            EnableDefault = enableDefault;
            DefaultExpression = defaultExpression;
            NoResultsTarget = noResultsTarget;
            PreventNoresultsCarry = preventNoresultsCarry;
            }
        //TODO: choose one of the modes?
        private FieldRenderer BuildFromWidget() {
            if (_widgetDefinition is DateWidgetDefinition) {
                var dateWidget = ((DateWidgetDefinition)_widgetDefinition);
                return new FieldRenderer(FieldRenderer.BaseRendererType.DATETIME.ToString().ToLower(), String.Format("time={0};format={1}", dateWidget.Time, dateWidget.Format), Attribute, null);
            }
            if (_widgetDefinition is NumberWidgetDefinition) {
                var numberWidget = ((NumberWidgetDefinition)_widgetDefinition);
                return new FieldRenderer(FieldRenderer.BaseRendererType.NUMERICINPUT.ToString().ToLower(), String.Format("min={0};max={1};decimals={2}", numberWidget.Min, numberWidget.Max, numberWidget.Decimals), Attribute, null);
            }
            return null;
        }


        public bool IsHidden {
            get; set;
        }

        public string DataType {
            get; set;
        }

        public string NoResultsTarget { get; set; }

        public bool PreventNoresultsCarry { get; set; }

        public FieldRenderer Renderer {
            get {
                return _renderer;
            }
            set {
                _renderer = value;
            }
        }

        public FieldFilter Filter {
            get {
                return _filter;
            }
            set {
                _filter = value;
            }
        }

        [Obsolete("Only used by unsupported Xamarin client code")]
        [JsonIgnore]
        public IWidgetDefinition WidgetDefinition {
            get {
                return _widgetDefinition;
            }
            set {
                _widgetDefinition = value;
            }
        }


        public bool IsAssociated {
            get {
                return Attribute.Contains(AttributeQualifierSeparator);
            }
        }

        public override string RendererType {
            get {
                return _renderer.RendererType;
            }
        }

        public IDictionary<string, object> RendererParameters {
            get {
                return _renderer == null ? new Dictionary<string, object>() : _renderer.ParametersAsDictionary();
            }
        }
        public IDictionary<string, object> FilterParameters {
            get {
                return _filter == null ? new Dictionary<string, object>() : _filter.ParametersAsDictionary();
            }
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
            return new ApplicationFieldDefinition(ApplicationName, Attribute, DataType, Label, RequiredExpression, IsReadOnly, IsHidden,
                Renderer, Filter, WidgetDefinition, DefaultValue, Qualifier, ShowExpression, ToolTip, AttributeToServer, _eventsSet, EnableExpression, EvalExpression,
                EnableDefault, DefaultExpression, DeclaredAsQueryOnEntity, NoResultsTarget, PreventNoresultsCarry);
        }

        public static ApplicationFieldDefinition HiddenInstance(string applicationName, string attributeName) {
            return new ApplicationFieldDefinition(applicationName, attributeName, null, "", "false", false, true,
                        new FieldRenderer(), new FieldFilter(), new HiddenWidgetDefinition(), null, null, null, null, null, null, null, null, null, null, false, null, false);
        }

        public static ApplicationFieldDefinition DefaultColumnInstance(string applicationName, string attributeName, string label) {
            return new ApplicationFieldDefinition(applicationName, attributeName, null, label, "false", false, false,
                        new FieldRenderer(), new FieldFilter(), new HiddenWidgetDefinition(), null, null, null, null, null, null, null, null, null, null, false, null, false);
        }

        public bool IsTransient() {
            return Attribute.StartsWith("#");
        }
    }
}