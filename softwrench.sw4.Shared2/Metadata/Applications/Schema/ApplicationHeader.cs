using cts.commons.portable.Util;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;

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

        public IEnumerable<IApplicationDisplayable> HeaderDisplayables { get; set; } = new List<IApplicationDisplayable>();


        public ApplicationHeader() {

        }

        public ApplicationHeader(string label, string parameters, string displacement, string showExpression, string helpIcon, string toolTip, IEnumerable<IApplicationDisplayable> displayables) {
            Label = label;
            Parameters = PropertyUtil.ConvertToDictionary(parameters);
            ValidateDisplacementType(displacement);
            _parametersString = parameters;
            ShowExpression = showExpression;
            HelpIcon = helpIcon;
            ToolTip = toolTip;
            HeaderDisplayables = displayables;
        }

        protected void ValidateDisplacementType(string displacement) {
            if (!string.IsNullOrWhiteSpace(displacement)) {
                DisplacementType result;
                if (!Enum.TryParse(displacement, true, out result)) {
                    throw new InvalidOperationException(string.Format(WrongRenderer, displacement));
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
            return new ApplicationHeader(Label, _parametersString, Displacement, ShowExpression, HelpIcon, ToolTip, HeaderDisplayables);
        }
    }
}
