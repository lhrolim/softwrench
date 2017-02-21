using System.Collections.Generic;
using cts.commons.simpleinjector;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.dataset {
    public class KongsbergDashboardWcProvider : ISingletonComponent {

        public readonly Dictionary<string, Dictionary<string, string>> DashBoardWhereClauses = new Dictionary<string, Dictionary<string, string>>() {
            {"servicerequest", new Dictionary<string, string>() {

                // applied to all sr grid dashboards - excludes anomalous sr with changedates way in the future
                {"", "SR.ticketid not in ('20587', '20588', '20590', '21582', '21583', '21585', '21588', '21591', '21592', '21593', '21594', '21595')" },

                {"closed", "sr.status in ('RESOLVED','CLOSED')" },
                {"open", "(SR.status = 'INPROG' or SR.status = 'SLAHOLD' or SR.status = 'NEW' or SR.status = 'PENDING' or SR.status = 'QUEUED')" }
            }}
        };
    }
}
