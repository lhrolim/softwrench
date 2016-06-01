using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.exception {

    public class ISMSyncException : Exception {

        public ISMSyncException() {

        }


        public ISMSyncException(string message):base(message) {

        }
    }
}
