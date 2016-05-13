using cts.commons.portable.Util;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace softwrench.sw4.Shared2.Metadata.Applications.Schema {
    public class ApplicationHeader : IPCLCloneable {
        private readonly string _parametersString;

        private const string WrongRenderer = "displacement {0} not found. Possible options are ontop,sameline";

        [DefaultValue("")]
        public string Label { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        public string Displacement { get { return DisplacementEnum.ToString().ToLower(); } }
        public DisplacementType DisplacementEnum { get; set; }
        public string HelpIcon { get; set; }
        public string ToolTip { get; set; }

        [DefaultValue("true")]
        public string ShowExpression { get; set; }


        public ApplicationHeader() {

        }

        public ApplicationHeader(string label, string parameters, string displacement, string showExpression, string helpIcon, string toolTip) {
            Label = label;
            Parameters = PropertyUtil.ConvertToDictionary(parameters);
            ValidateDisplacementType(displacement);
            _parametersString = parameters;
            ShowExpression = showExpression;
            HelpIcon = helpIcon;
            ToolTip = toolTip;
        }

        protected void ValidateDisplacementType(String displacement) {
            DisplacementType result;
            if (!String.IsNullOrWhiteSpace(displacement)) {
                if (!Enum.TryParse(displacement, true, out result)) {
                    throw new InvalidOperationException(String.Format(WrongRenderer, displacement));
                }
                DisplacementEnum = result;
            } else {
                DisplacementEnum = DisplacementType.SAMELINE;
            }
        }

        public enum DisplacementType {
            ONTOP, SAMELINE
        }
        public override string ToString() {
            return string.Format("Label: {0}, Displacement: {1}, Abstract: {2}", Label, Displacement);
        }

        public object Clone() {
            return new ApplicationHeader(Label, _parametersString, Displacement,ShowExpression, HelpIcon, ToolTip);
        }
    }
}
