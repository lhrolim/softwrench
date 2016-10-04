using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using softwrench.sW4.Shared2.Metadata.Applications.UI;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema {

    /// <summary>
    /// Type of Field which is obrigatory a combo with a fixed list of options (AssociationOption).<p/> 
    /// 
    /// It´s represented by the <optionfield></optionfield> tag in the metadata.xml
    /// 
    /// </summary>
    public class OptionField : BaseApplicationFieldDefinition, IDataProviderContainer, IDependableField, IPCLCloneable {

        private readonly List<IAssociationOption> _options;
        private readonly OptionFieldRenderer _renderer;
        private readonly FieldFilter _filter;
        private readonly String _providerAttribute;
        private readonly ISet<string> _dependantFields = new HashSet<string>();
        private bool _sort;
        private ISet<ApplicationEvent> _eventsSet;
        private string _dependantFieldsString;

        public String EnableExpression { get; set; }


        public OptionField(string applicationName, string label, string attribute, string qualifier, string requiredExpression, bool isReadOnly, bool isHidden,
            OptionFieldRenderer renderer, FieldFilter filter, List<IAssociationOption> options, string defaultValue, bool sort, string showExpression,
            string toolTip, string attributeToServer, ISet<ApplicationEvent> events, string providerAttribute, string dependantFields, string enableExpression)
            : base(applicationName, label, attribute, requiredExpression, isReadOnly, defaultValue, qualifier, showExpression, toolTip, attributeToServer, events, enableExpression) {
            _renderer = renderer;
            _filter = filter;
            _options = options;
            _providerAttribute = providerAttribute;
            IsHidden = isHidden;
            _sort = sort;
            if (sort) {
                if (_options != null) {
                    _options.Sort();
                }
            }
            _eventsSet = events;
            _dependantFieldsString = dependantFields;
            _dependantFields = ParsingUtil.GetCommaSeparetedParsingResults(dependantFields);
            EnableExpression = enableExpression;

        }

        public bool IsHidden { get; set; }

        public override IDictionary<string, string> RendererParameters {
            get { return _renderer == null ? new Dictionary<string, string>() : _renderer.ParametersAsDictionary(); }
        }

        public FieldFilter Filter {
            get { return _filter; }
        }

        public IDictionary<string, string> FilterParameters {
            get { return _filter == null ? new Dictionary<string, string>() : _filter.ParametersAsDictionary(); }
        }

        public override string RendererType { get { return _renderer.RendererType.ToLower(); } }


        public string AssociationKey { get { return _providerAttribute ?? Attribute; } }
        public string Target { get { return Attribute; } }

        public string ProviderAttribute {
            get { return _providerAttribute; }
        }


        public IEnumerable<IAssociationOption> Options {
            get { return _options; }
        }

        public ISet<string> DependantFields {
            get { return _dependantFields; }
        }

        public bool Sort {
            get { return _sort; }
        }

        public object Clone() {
            return new OptionField(ApplicationName, Label, Attribute, Qualifier, RequiredExpression, IsReadOnly, IsHidden, _renderer, _filter,
                _options,
                DefaultValue, _sort, ShowExpression, ToolTip, AttributeToServer, _eventsSet, ProviderAttribute,
                _dependantFieldsString, EnableExpression);
        }

        public override string ToString() {
            return string.Format("ProviderAttribute: {0}, Target: {1}", _providerAttribute, Target);
        }
    }
}
