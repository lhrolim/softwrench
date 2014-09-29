using System.Collections.Generic;
using softWrench.sW4.Metadata.Applications.Schema;
using softwrench.sW4.Shared.Metadata.Applications.Association;

namespace softwrench.sW4.Shared.Metadata.Applications.Schema {

    /// <summary>
    /// Type of Field which is obrigatory a combo with a fixed list of options (AssociationOption).<p/> 
    /// 
    /// It´s represented by the <optionfield></optionfield> tag in the metadata.xml
    /// 
    /// </summary>
    public class OptionField : BaseApplicationFieldDefinition, IDataProviderContainer {

        private readonly List<IAssociationOption> _options;

        public OptionField(string applicationName, string label, string attribute,
            bool isRequired, bool isReadOnly, List<IAssociationOption> options, string defaultValue, bool sort, string showExpression, string toolTip)
            : base(applicationName, label, attribute, isRequired, isReadOnly, defaultValue, null, showExpression, toolTip) {
            _options = options;
            if (sort) {
                _options.Sort();
            }
        }

        public override string RendererType { get { return "combo"; } }

        public string AssociationKey { get { return Attribute; } }
        public string Target { get { return Attribute; } }

        public IEnumerable<IAssociationOption> Options {
            get { return _options; }
        }
    }
}
