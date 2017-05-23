using System;
using System.Collections.Generic;
using softwrench.sw4.api.classes.audit;
using softwrench.sW4.audit.classes.Model;
using softwrench.sW4.audit.Interfaces;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Services;

namespace softwrench.sW4.audit.classes.Services {
    public class AuditManager : IAuditManager {
        public static SWDBHibernateDAO DAO {
            get {
                return SWDBHibernateDAO.GetInstance();
            }
        }

        public AuditEntry CreateAuditEntry(string action, string refApplication, string refId, string refUserId, string data, DateTime createdDate) {
            var user = SecurityFacade.CurrentUser();
            AuditEntry auditEntry = new AuditEntry(action, refApplication, refId, refUserId, data, user.Login, createdDate);
            return SaveAuditEntry(auditEntry);
        }

        public AuditEntry SaveAuditEntry(AuditEntry auditEntry) {
            return DAO.Save(auditEntry);
        }

        public AuditEntry FindById(int auditId) {
            AuditEntry auditEntry = DAO.FindByPK<AuditEntry>(typeof(AuditEntry), auditId);
            return auditEntry;
        }

        public ICollection<AuditEntry> SaveAuditEntries(ICollection<AuditEntry> entries) {
            return DAO.BulkSave(entries);
        }

        void IAuditManagerCommons.CreateAuditEntry(string action, string refApplication, string refId, string refUserId, string data) {
            CreateAuditEntry(action, refApplication, refId, refUserId, data, DateTime.Now);
        }
    }
}
