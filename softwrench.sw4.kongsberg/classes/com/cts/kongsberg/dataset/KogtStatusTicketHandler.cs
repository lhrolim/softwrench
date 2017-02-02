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

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.dataset {


    /// <summary>
    /// Implementing SWWEB-2916
    /// </summary>
    [OverridingComponent(ClientFilters = "kongsberg")]
    public class KogtStatusTicketHandler : TicketStatusHandler{
        public KogtStatusTicketHandler(IContextLookuper contextLookuper) : base(contextLookuper)
        {
        }

        public override ISet<IAssociationOption> DoFilterAvailableStatus(AttributeHolder originalEntity, MaxSrStatus statusEnum, ISet<IAssociationOption> filterAvailableStatus) {
            var available = base.DoFilterAvailableStatus(originalEntity, statusEnum, filterAvailableStatus);

            var slaHoldStatus = HandleSlaHoldStatus(originalEntity, statusEnum);

            if (statusEnum.Equals(MaxSrStatus.QUEUED)) {
                available.Clear();
                //Closed, In Progress, Pending, Duplicate, Resolved, Spam, SLA Hold 
                available.AddAll(filterAvailableStatus.Where(f => f.Value.EqualsAny(MaxSrStatus.CLOSED,MaxSrStatus.INPROG, MaxSrStatus.PENDING, MaxSrStatus.DUPLICATE, MaxSrStatus.RESOLVED, MaxSrStatus.SPAM, slaHoldStatus)));
            }
            if (statusEnum.Equals(MaxSrStatus.SLAHOLD)) {
                available.Clear();
                //  Closed, In Progress, Pending, Queued, Duplicate, Resolved, Spam
                available.AddAll(filterAvailableStatus.Where(f => f.Value.EqualsAny(MaxSrStatus.CLOSED, MaxSrStatus.INPROG, MaxSrStatus.PENDING, MaxSrStatus.QUEUED, MaxSrStatus.DUPLICATE, MaxSrStatus.RESOLVED, MaxSrStatus.SPAM)));
            }

            if (statusEnum.Equals(MaxSrStatus.INPROG)) {
                available.Clear();
                //Closed, Pending, Queued, Duplicate, Resolved, Spam, SLA Hold
                available.AddAll(filterAvailableStatus.Where(f => f.Value.EqualsAny(MaxSrStatus.CLOSED, MaxSrStatus.PENDING, MaxSrStatus.QUEUED, MaxSrStatus.DUPLICATE, MaxSrStatus.RESOLVED, MaxSrStatus.SPAM, slaHoldStatus)));
            }

            if (statusEnum.Equals(MaxSrStatus.RESOLVED)) {
                available.Clear();
                //Closed, In Progress, Pending, Queued, Duplicate Spam
                available.AddAll(filterAvailableStatus.Where(f => f.Value.EqualsAny(MaxSrStatus.CLOSED, MaxSrStatus.INPROG,MaxSrStatus.PENDING, MaxSrStatus.QUEUED,  MaxSrStatus.DUPLICATE, MaxSrStatus.SPAM)));
            }

            return available;
        }
    }
}
