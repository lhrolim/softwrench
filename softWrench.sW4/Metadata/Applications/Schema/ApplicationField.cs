using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using softWrench.sW4.Metadata.Applications.UI;
using softWrench.sW4.Metadata.Entities.Schema;

namespace softWrench.sW4.Metadata.Applications.Schema {

    public class ApplicationField : BaseApplicationField {

        public const string AttributeQualifierSeparator = EntityAttribute.AttributeQualifierSeparator;

        private readonly bool _isHidden;
        private readonly FieldRenderer _renderer;
        private readonly IWidget _widget;
        //        private readonly IDictionary<String, String> _rendererParameters;


        public ApplicationField(string applicationName, [NotNull] string attribute, [NotNull] string label, bool isRequired, bool isReadOnly, bool isIsHidden,
            [NotNull] FieldRenderer renderer, [NotNull] IWidget widget, [CanBeNull]string defaultValue, [CanBeNull]string qualifier, string showExpression, string toolTip)
            : base(applicationName, label, attribute, isRequired, isReadOnly, defaultValue, qualifier, showExpression, toolTip) {
            if (widget == null) throw new ArgumentNullException("widget");
            _widget = widget;
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
            if (_widget is DateWidget) {
                var dateWidget = ((DateWidget)_widget);
                return new FieldRenderer(FieldRenderer.BaseRendererType.DATETIME.ToString().ToLower(), String.Format("time={0};format={1}", dateWidget.Time, dateWidget.Format), _attribute);
            } else if (_widget is NumberWidget) {
                var numberWidget = ((NumberWidget)_widget);
                return new FieldRenderer(FieldRenderer.BaseRendererType.NUMERICINPUT.ToString().ToLower(), String.Format("min={0};max={1};decimals={2}", numberWidget.Min, numberWidget.Max, numberWidget.Decimals), _attribute);
            }
            return null;
        }


        public bool IsHidden {
            get { return _isHidden; }
        }

        [NotNull]
        public IWidget Widget {
            get { return _widget; }
        }

        public bool IsAssociated {
            get {
                return _attribute.Contains(AttributeQualifierSeparator);
            }
        }



        public override string RendererType {
            get { return _renderer.RendererType; }
        }

        //        public HashSet<ApplicationField> DependantFields {
        //            get { return AssociationMetadata.DependantFields; }
        //        }

        public IDictionary<string, string> RendererParameters {
            get { return _renderer.ParametersAsDictionary(); }
        }

        //        [JsonIgnore]
        //        public AssociationMetadata AssociationMetadata {
        //            get { return _renderer.AssociationMetadata; }
        //        }

        protected bool Equals(ApplicationField other) {
            return string.Equals(_attribute, other._attribute);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ApplicationField)obj);
        }

        public override int GetHashCode() {
            return (_attribute != null ? _attribute.GetHashCode() : 0);
        }

        public override string ToString() {
            return Tuple.Create("attr:", Attribute, "label", Label).ToString();
        }

        //        public void FetchOptions() {
        //            AssociationMetadata.FetchOptions();
        //        }
    }
}