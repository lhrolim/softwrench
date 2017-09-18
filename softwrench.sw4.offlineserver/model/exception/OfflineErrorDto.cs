using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using softWrench.sW4.Exceptions;

namespace softwrench.sw4.offlineserver.model.exception {
    public class OfflineErrorDto : ErrorDto {

        public bool RequestSupportReport { get; set; }

        public OfflineErrorDto([NotNull] Exception rootException) : base(rootException) {
            if (rootException is IOfflineSyncException) {
                RequestSupportReport = ((IOfflineSyncException)rootException).RequestSupportReport;
            }
        }

    }
}
