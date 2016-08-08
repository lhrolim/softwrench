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

    public class DeployValidationResult {
        private List<DeployValidationModel> validationResultList = null;

        /// <summary>
        /// True if the test data is missing; otherwise false
        /// </summary>
        public bool MissingTestData { get; set; }

        public List<DeployValidationModel> ValidationResultList {
            get {
                return validationResultList ?? (validationResultList = new List<DeployValidationModel>());
            }
            set {
                validationResultList = value;
            }
        }
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
        /// The validation action name
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The validation action description
        /// </summary>
        public string ActionDescription { get; set; }

        /// <summary>
        /// The schema key for the application
        /// </summary>
        public string SchemaKey { get; set; }

        /// <summary>
        /// The validation action is supported
        /// </summary>
        public bool ActionSupported { get; set; }

        /// <summary>
        /// The application metadata
        /// </summary>
        public CompleteApplicationMetadataDefinition Metadata { get; set; }

        /// <summary>
        /// The composition data
        /// </summary>
        public CompositionOperationDTO CompositionOperationDTO { get; set; }

        /// <summary>
        /// The Json for the operation
        /// </summary>
        public JObject TestJson { get; set; }
        
        /// <summary>
        /// True if the test data is missing.
        /// </summary>
        public bool IsMissingTestData { get; set; }
    } 
}
