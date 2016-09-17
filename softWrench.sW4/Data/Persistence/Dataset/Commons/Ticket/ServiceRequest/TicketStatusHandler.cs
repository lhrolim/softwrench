using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Security.Context;
using s = softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest.MaxSrStatus;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest {
    public class TicketStatusHandler : ISingletonComponent {


        private readonly IContextLookuper _contextLookuper;

        public TicketStatusHandler(IContextLookuper contextLookuper) {
            _contextLookuper = contextLookuper;
        }


        public IEnumerable<IAssociationOption> FilterAvailableStatus(AssociationPostFilterFunctionParameters postFilter) {
            var metadataParameters = _contextLookuper.LookupContext().MetadataParameters;
            string currentStatus = null;

            if (postFilter.OriginalEntity.ContainsAttribute("originalstatus")) {
                currentStatus = postFilter.OriginalEntity.GetAttribute("originalstatus").ToString();
            } else if (metadataParameters.ContainsKey("currentstatus")) {
                currentStatus = metadataParameters["currentstatus"].ToString();
            }
            var filterAvailableStatus = postFilter.Options;
            if (currentStatus == null) {
                return new List<IAssociationOption> { filterAvailableStatus.First(l => l.Value.EqualsIc("OPEN")) };
            }
            var currentOption = filterAvailableStatus.FirstOrDefault(l => l.Value.Equals(currentStatus));

            if (currentStatus.EqualsIc(s.CANCELLED.ToString())) {

                if (filterAvailableStatus.Count > 1) {
                    throw new LookupEmptySelectionException("Cancelled Status cannot be changed");
                }
                return filterAvailableStatus;

            }

            var baseResult = new List<IAssociationOption>();
            if (currentOption != null) {
                baseResult.Add(currentOption);
            }


            MaxSrStatus statusEnum;
            Enum.TryParse(currentStatus, true, out statusEnum);

            switch (statusEnum) {
                case s.NEW:

                baseResult.AddRange(filterAvailableStatus.Where(l => l.Value.EqualsAny(s.CANCELLED, s.QUEUED, s.REJECTED)));
                break;

                case s.QUEUED:

                baseResult.AddRange(filterAvailableStatus.Where(l => l.Value.EqualsAny(s.INPROG, s.REJECTED, s.CANCELLED)));
                break;

                case s.INPROG:
                baseResult.AddRange(
                    filterAvailableStatus.Where(
                        l => l.Value.EqualsAny(s.CANCELLED, s.PENDING, s.QUEUED, s.REJECTED, s.RESOLVCONF, s.RESOLVED, s.SLAHOLD)));
                break;

                case s.CLOSED:
                baseResult.AddRange(filterAvailableStatus.Where(l => l.Value.EqualsAny(s.CANCELLED)));
                break;

                case s.RESOLVED:
                baseResult.AddRange(
                    filterAvailableStatus.Where(
                        l => l.Value.EqualsAny(s.CLOSED, s.CANCELLED, s.INPROG, s.QUEUED, s.REJECTED, s.RESOLVCONF)));
                break;

                case s.COMP:
                baseResult.AddRange(filterAvailableStatus.Where(l => l.Value.EqualsAny(s.CLOSED, s.CANCELLED)));
                break;

                case s.PENDING:
                baseResult.AddRange(filterAvailableStatus.Where(l => l.Value.EqualsAny(s.CANCELLED, s.INPROG, s.QUEUED, s.REJECTED, s.RESOLVCONF, s.RESOLVED, s.SLAHOLD)));
                break;

                case s.SLAHOLD:
                baseResult.AddRange(filterAvailableStatus.Where(l => l.Value.EqualsAny(s.CANCELLED, s.INPROG, s.QUEUED, s.REJECTED, s.RESOLVCONF, s.RESOLVED, s.PENDING)));
                break;

            }

            return baseResult;
        }

    }
}
