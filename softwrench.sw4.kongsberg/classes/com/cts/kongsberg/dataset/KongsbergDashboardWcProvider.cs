using System.Collections.Generic;
using cts.commons.simpleinjector;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.dataset {
    public class KongsbergDashboardWcProvider : ISingletonComponent {

        public readonly Dictionary<string, Dictionary<string, string>> DashBoardWhereClauses = new Dictionary<string, Dictionary<string, string>>() {
            {"servicerequest", new Dictionary<string, string>() {
                {"closed", "sr.status in ('RESOLVED','CLOSED')" },
                {"open", "(SR.status = 'INPROG' or SR.status = 'SLAHOLD' or SR.status = 'NEW' or SR.status = 'PENDING' or SR.status = 'QUEUED')" }
            }}
        };
    }
}
