using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata;
using softWrench.sW4.Data.API.Composition;
using System;
using System.Collections.Generic;

namespace softWrench.sW4.Util.DeployValidation
{
    public class DeployValidationModel {
        private Exception _ex = null;
        public HashSet<string> MissingProperties { get; set; }
        public bool MissingWsdl { get; set; }
       
        public string ExClassName { get; set; }
        public string ExMsg { get; set; }
        public string ExStack { get; set; }
        public bool HasProblems { get; set; }

        public bool MissingTestData { get; set; }

        /// <summary>
        /// The name of the current validation action
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// The name of the current validation action description
        /// </summary>
        public string ActionDescription { get; set; }

        /// <summary>
        /// The validation action is supported
        /// </summary>
        public bool ActionSupported { get; set; }

        public DeployValidationModel() {
            MissingProperties = new HashSet<string>();
        }

        public void AddMissingProperty(string property) {
            MissingProperties.Add(property);
        }

        public void ReportException(Exception ex) {
            _ex = ex;
            ExClassName = ex.GetType().Name;
            ExMsg = ex.Message;
            ExStack = ex.StackTrace;
        }

        public bool CalcHasProblems() {
            return HasProblems = MissingProperties.Count > 0 || MissingWsdl || _ex != null;
        }
    }
}
