using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest;
using softWrench.sW4.Security.Context;

namespace softwrench.sw4.pesco.classes.com.cts.pesco.dataset {

    [OverridingComponent(ClientFilters = "pesco")]
    public class PescoTicketStatusHandler : TicketStatusHandler {

        public PescoTicketStatusHandler(IContextLookuper contextLookuper) : base(contextLookuper) {
        }

        protected override IList<IAssociationOption> DoFilterAvailableStatus(MaxSrStatus statusEnum, ISet<IAssociationOption> filterAvailableStatus) {
            var available = base.DoFilterAvailableStatus(statusEnum, filterAvailableStatus);
            if (statusEnum.Equals(MaxSrStatus.NEW)) {
                available.Add(filterAvailableStatus.FirstOrDefault(f => f.Value.EqualsAny(MaxSrStatus.PENDING, MaxSrStatus.INPROG)));
            }
            return available;
        }
    }
}
