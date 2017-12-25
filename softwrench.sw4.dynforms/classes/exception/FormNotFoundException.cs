using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.dynforms.classes.exception {
    public class FormNotFoundException : InvalidOperationException {

        public FormNotFoundException(int formId):base($"Could not locate form with id ${formId}"){
            
        }

    }
}
