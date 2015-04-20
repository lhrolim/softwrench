using System;
using cts.commons.simpleinjector;
using softwrench.sW4.audit.classes.Model;
using softWrench.sW4.Data.Persistence.SWDB;

namespace softwrench.sW4.audit.classes.Services {
    public class AuditManager : ISingletonComponent
    {
        public static SWDBHibernateDAO DAO {
            get {
                return SWDBHibernateDAO.GetInstance();
            }
        }

        public static AuditEntry CreateAuditEntry(string action, string refApplication, string refId, string data, string createdBy, DateTime createdDate)
        {
            AuditEntry auditEntry = new AuditEntry(action, refApplication, refId, data, createdBy, createdDate);
            return SaveAuditEntry(auditEntry);
        }

        public static AuditEntry SaveAuditEntry(AuditEntry auditEntry) {
            return DAO.Save(auditEntry);
        }

        public static AuditEntry FindById(int auditId)
        {
            AuditEntry auditEntry = DAO.FindByPK<AuditEntry>(typeof(AuditEntry), auditId);
            return auditEntry;
        }
    }
}
