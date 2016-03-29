using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.Entities.Workflow {
    public class MaximoWorkflowException : Exception {
        public MaximoWorkflowException(string message) : base(message) {
        }
    }
}
