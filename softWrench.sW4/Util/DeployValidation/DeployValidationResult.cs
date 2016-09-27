using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Util.DeployValidation {
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
}
