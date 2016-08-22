using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using JetBrains.Annotations;

namespace softWrench.sW4.Scheduler.Interfaces {
    public class SwJobException : Exception {

        public SwJobException() {

        }

        [StringFormatMethod("msg")]
        public SwJobException(string msg, params object [] parameters) : base(msg.Fmt(parameters)) {

        }
    }
}
