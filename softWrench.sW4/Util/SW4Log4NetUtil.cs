using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using log4net;

namespace softWrench.sW4.Util {
    public static class Sw4Log4NetUtil {

        [StringFormatMethod("msg")]
        [System.Diagnostics.DebuggerHidden]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void DebugOrInfoFormat(this ILog log, string msg, params object[] parameters) {
            if (ApplicationConfiguration.IsLocal()) {
                log.InfoFormat(msg, parameters);
            } else {
                log.DebugFormat(msg, parameters);
            }

        }

    }
}
