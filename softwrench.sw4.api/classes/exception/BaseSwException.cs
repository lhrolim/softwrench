using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.api.classes.exception {
    public abstract class BaseSwException : Exception {

        public virtual bool NotifyException {
            get { return true; }
        }

    }
}
