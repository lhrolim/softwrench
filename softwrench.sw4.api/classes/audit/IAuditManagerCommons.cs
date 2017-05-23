using System;
using cts.commons.simpleinjector;

namespace softwrench.sw4.api.classes.audit {
    public interface IAuditManagerCommons : ISingletonComponent {

        /// <summary>
        /// Creates a new AuditEntry and saves it to the SWDB
        /// </summary>
        /// <param name="action"></param>
        /// <param name="refApplication"></param>
        /// <param name="refId"></param>
        /// <param name="refUserId"></param>
        /// <param name="data"></param>
        /// <returns>AuditEntry</returns>
        void CreateAuditEntry(string action, string refApplication, string refId, string refUserId, string data);
    }
}