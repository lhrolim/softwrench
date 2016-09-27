using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Util.DeployValidation {
    public class DirectoryAndFileValidationModel {
        public string Name { get; set; }

        public string Path { get; set; }
        
        public List<DirectoryAndFileValidation> Validations { get; set; }
    }

    public class DirectoryAndFileValidation {
        public bool ValidationSuccess { get; set; }

        public string ValidationMessage { get; set; }
    }
}
