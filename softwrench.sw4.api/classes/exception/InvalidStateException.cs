using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.api.classes.exception {

    public class InvalidStateException : Exception {

        public InvalidStateException() {

        }

        public InvalidStateException(string message) : base(message) {

        }
    }
}
