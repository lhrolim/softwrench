using cts.commons.portable.Util;
using System;
using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace softwrench.sw4.Shared2.Metadata.Applications.UI {
    public class FieldFilter {
        private const string WrongOperation = "filter operation {0} not found. Possible operation are contains, ncontains, startwith, endwith, eq, btw, noteq, gt, lt, gte and lte";

        public FieldFilter() {
        }

        public FieldFilter(string operation, string parameters, string defaultValue, string targetName, string clientFunction) {
            Parameters = parameters;
            TargetName = targetName;
            Operation = operation;
            Default = defaultValue;
            ClientFunction = clientFunction;
            if (operation != null) {
                ValidateOperation(operation);
            }
        }

        protected void ValidateOperation(string operation) {
            BaseOperationType result;
            if (!Enum.TryParse(operation, true, out result)) {
                throw new InvalidOperationException(string.Format(WrongOperation, operation));
            }
        }

        public string ClientFunction {
            get; set;
        }

        public string Default {
            get; set;
        }

        private string TargetName {
            get; set;
        }

        public string Operation {
            get; set;
        }

        public string Parameters {
            get; set;
        }

        public enum BaseOperationType {
            CONTAINS, NCONTAINS, STARTWITH, ENDWITH, EQ, BTW, NOTEQ, GT, LT, GTE, LTE
        }

        public IDictionary<string, object> ParametersAsDictionary() {
            return PropertyUtil.ConvertToDictionary(Parameters);
        }

        public override string ToString() {
            return string.Format("TargetName: {0}, Operation: {1}, Parameters: {2}", TargetName, Operation, Parameters);
        }
    }
}
