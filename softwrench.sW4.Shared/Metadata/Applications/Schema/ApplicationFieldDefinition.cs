using System;
using System.Collections.Generic;
using softwrench.sW4.Shared.Metadata.Applications.UI;

namespace softwrench.sW4.Shared.Metadata.Applications.Schema {

    public class ApplicationFieldDefinition : BaseApplicationFieldDefinition,IDefinition {

        public const string AttributeQualifierSeparator = ".";

        private readonly bool _isHidden;
        private readonly FieldRenderer _renderer;
        private readonly IWidgetDefinition _widgetDefinition;
        public IDictionary<string, object> ExtensionParameters { get; set; }
        //        private readonly IDictionary<String, String> _rendererParameters;


        public ApplicationFieldDefinition(string applicationName,  string attribute,  string label, bool isRequired, bool isReadOnly, bool isIsHidden,
             FieldRenderer renderer,  IWidgetDefinition widgetDefinition, string defaultValue, string qualifier, string showExpression, string toolTip)
            : base(applicationName, label, attribute, isRequired, isReadOnly, defaultValue, qualifier, showExpression, toolTip) {
            if (widgetDefinition == null) throw new ArgumentNullException("widgetDefinition");
            _widgetDefinition = widgetDefinition;
            _renderer = renderer;
            _isHidden = isIsHidden;
            if (String.IsNullOrEmpty(renderer.RendererType)) {
                var newRenderer = BuildFromWidget();
                if (newRenderer != null) {
                    _renderer = newRenderer;
                }
            }

        }
        //TODO: choose one of the modes?
        private FieldRenderer BuildFromWidget() {
            if (_widgetDefinition is DateWidgetDefinition) {
                var dateWidget = ((DateWidgetDefinition)_widgetDefinition);
                return new FieldRenderer(FieldRenderer.BaseRendererType.DATETIME.ToString(), String.Format("time={0};format={1}", dateWidget.Time, dateWidget.Format), Attribute);
            } else if (_widgetDefinition is NumberWidgetDefinition) {
                var numberWidget = ((NumberWidgetDefinition)_widgetDefinition);
                return new FieldRenderer(FieldRenderer.BaseRendererType.NUMERICINPUT.ToString(), String.Format("min={0};max={1};decimals={2}", numberWidget.Min, numberWidget.Max, numberWidget.Decimals), Attribute);
            }
            return null;
        }


        public bool IsHidden {
            get { return _isHidden; }
        }

        
        public IWidgetDefinition WidgetDefinition {
            get { return _widgetDefinition; }
        }



        public bool IsAssociated {
            get {
                return Attribute.Contains(AttributeQualifierSeparator);
            }
        }



        public override string RendererType {
            get { return _renderer.RendererType; }
        }



        //        public HashSet<ApplicationFieldDefinition> DependantFields {
        //            get { return AssociationMetadata.DependantFields; }
        //        }

        public IDictionary<string, string> RendererParameters {
            get { return _renderer.ParametersAsDictionary(); }
        }

        //        [JsonIgnore]
        //        public AssociationMetadata AssociationMetadata {
        //            get { return _renderer.AssociationMetadata; }
        //        }

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

        //        public void FetchOptions() {
        //            AssociationMetadata.FetchOptions();
        //        }
        
    }
}