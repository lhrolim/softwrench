using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Metadata.Applications.DataSet.Filter {

    /// <summary>
    /// USe this exception to prvent a lookup from showing up throwing the given message instead
    /// </summary>
    public class LookupEmptySelectionException : Exception {


        public LookupEmptySelectionException(string message = null) : base(message) {

        }

    }
}
