using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata;
using System;
using System.Collections.Generic;

namespace softWrench.sW4.Util.DeployValidation
{
    public class DeployValidationModel {
        private Exception _ex = null;
        public HashSet<string> MissingProperties { get; set; }
        public bool MissingWsdl { get; set; }
        public bool MissingTestData { get; set; }
        public string ExClassName { get; set; }
        public string ExMsg { get; set; }
        public string ExStack { get; set; }
        public bool HasProblems { get; set; }

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
            return HasProblems = MissingProperties.Count > 0 || MissingWsdl || MissingTestData || _ex != null;
        }
    }

    public class DeployValidationResult {
        public DeployValidationModel CreateValidation { get; set; }
        public DeployValidationModel UpdateValidation { get; set; }
    }

    /// <summary>
    /// The model holding the json information for applications.
    /// </summary>
    public class ApplicationValidationModel {

        /// <summary>
        /// The application name
        /// </summary>
        public string Application { get; set; }

        /// <summary>
        /// The application metadata
        /// </summary>
        public CompleteApplicationMetadataDefinition Metadata { get; set; }

        /// <summary>
        /// The Json for the create operation
        /// </summary>
        public JObject CreateJson { get; set; }

        /// <summary>
        /// The Json for the update operation
        /// </summary>
        public JObject UpdateJson { get; set; }

        /// <summary>
        /// True if the test data is missing.
        /// </summary>
        public bool IsMissingTestData { get; set; }
    } 
}
