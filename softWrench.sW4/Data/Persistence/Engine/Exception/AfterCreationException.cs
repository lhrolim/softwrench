using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.WS.API;

namespace softWrench.sW4.Data.Persistence.Engine.Exception {

    public class AfterCreationException : System.Exception {
        private readonly TargetResult _resultObject;

        public AfterCreationException(TargetResult resultObject, System.Exception e, string message=null) : base(message, e) {
            _resultObject = resultObject;
        }

        public TargetResult ResultObject {
            get {
                return _resultObject;
            }
        }
    }
}
