using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Exceptions {

    public class MissingConfigurationException : Exception {

        public MissingConfigurationException(string message)  : base(message){

        }

    }
}
