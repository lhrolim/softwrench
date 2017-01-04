using NHibernate;

namespace cts.commons.persistence.Util {
    public class TransactionPair {
        public TransactionPair(ISession session, ITransaction transaction) {
            Session = session;
            Transaction = transaction;
        }
        public ISession Session { get; private set; }
        public ITransaction Transaction { get; private set; }
    }
}
