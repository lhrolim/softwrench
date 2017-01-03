using NHibernate;

namespace cts.commons.persistence.Transaction {
    public class TransactionContext {
        public int TransactionCounter { get; set; }
        public ITransaction Transaction { get; set; }
        public ISession Session { get; set; }
        public bool TransactionManaged { get; set; }
        public DBType DbType { get; set; }
    }
}
