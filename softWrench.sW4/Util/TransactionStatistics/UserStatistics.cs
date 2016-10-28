using softWrench.sW4.Audit;
using softWrench.sW4.Data.Entities.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Util.TransactionStatistics {
    public class UserStatistics {
        
        private List<AuditTrail> transactions;

        public int UserId { get; set; }

        public string FullName { get; set; }

        public bool IsActive { get; set; }

        public int LoginCount { get; set; }

        public List<AuditTrail> Transactions {
            get {
                return transactions ?? (transactions = new List<AuditTrail>());
            }

            set {
                transactions = value;
            }
        }
    }
}
