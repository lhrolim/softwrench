using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata;
using softWrench.sW4.Data.API.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Util.DeployValidation {
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
