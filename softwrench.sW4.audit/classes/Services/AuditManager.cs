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

        public static AuditEntry SaveAuditEntry(AuditEntry auditEntry) {
            return DAO.Save(auditEntry);
        }
    }
}
