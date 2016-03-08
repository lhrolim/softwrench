using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Metadata.Security {

    public class SecurityException : Exception {

        public SecurityException(string message) : base(message) {
        }
    }
}
