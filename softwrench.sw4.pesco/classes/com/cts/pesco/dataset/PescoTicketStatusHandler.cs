using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;

namespace softwrench.sw4.pesco.classes.com.cts.pesco.dataset {

    [OverridingComponent(ClientFilters = "pesco")]
    public class PescoTicketStatusHandler : TicketStatusHandler {

        public PescoTicketStatusHandler(IContextLookuper contextLookuper) : base(contextLookuper) {
        }

        public override ISet<IAssociationOption> DoFilterAvailableStatus(AttributeHolder originalEntity, MaxSrStatus statusEnum, ISet<IAssociationOption> filterAvailableStatus) {
            var available = base.DoFilterAvailableStatus(originalEntity,statusEnum, filterAvailableStatus);
            if (statusEnum.Equals(MaxSrStatus.NEW)) {
                available.AddAll(filterAvailableStatus.Where(f => f.Value.EqualsAny(MaxSrStatus.PENDING, MaxSrStatus.INPROG)));
            }
            return available;
        }
    }
}
