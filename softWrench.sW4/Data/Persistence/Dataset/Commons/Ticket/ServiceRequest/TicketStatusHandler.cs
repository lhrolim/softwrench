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

            var addcurrent = true;
            if (postFilter.OriginalEntity.ContainsAttribute("addcurrent")) {
                addcurrent = bool.Parse(postFilter.OriginalEntity.GetStringAttribute("addcurrent"));
            }
            

            if (postFilter.OriginalEntity.ContainsAttribute("originalstatus")) {
                currentStatus = postFilter.OriginalEntity.GetAttribute("originalstatus").ToString();
            } else if (metadataParameters.ContainsKey("currentstatus")) {
                currentStatus = metadataParameters["currentstatus"].ToString();
            }
            var filterAvailableStatus = postFilter.Options;
            if (currentStatus == null) {
                return filterAvailableStatus;
            }
            var currentOption = filterAvailableStatus.FirstOrDefault(l => l.Value.Equals(currentStatus));

            if (currentStatus.EqualsIc(s.CANCELLED.ToString())) {

                if (filterAvailableStatus.Count > 1) {
                    throw new LookupEmptySelectionException("Cancelled Status cannot be changed");
                }
                return filterAvailableStatus;

            }

            var baseResult = new List<IAssociationOption>();
            if (currentOption != null && addcurrent) {
                baseResult.Add(currentOption);
            }


            MaxSrStatus statusEnum;
            Enum.TryParse(currentStatus, true, out statusEnum);

            baseResult.AddRange(DoFilterAvailableStatus(statusEnum, filterAvailableStatus));

            return baseResult;
        }

        protected virtual IList<IAssociationOption> DoFilterAvailableStatus(MaxSrStatus statusEnum, ISet<IAssociationOption> filterAvailableStatus) {
            switch (statusEnum) {
                case s.NEW:

                return filterAvailableStatus.Where(l => l.Value.EqualsAny(s.CANCELLED, s.QUEUED, s.REJECTED)).ToList();


                case s.QUEUED:

                return filterAvailableStatus.Where(l => l.Value.EqualsAny(s.INPROG, s.REJECTED, s.CANCELLED)).ToList();

                case s.INPROG:
                return
                    filterAvailableStatus.Where(
                        l =>
                            l.Value.EqualsAny(s.CANCELLED, s.PENDING, s.QUEUED, s.REJECTED, s.RESOLVCONF, s.RESOLVED,
                                s.SLAHOLD)).ToList();

                case s.CLOSED:
                return filterAvailableStatus.Where(l => l.Value.EqualsAny(s.CANCELLED)).ToList();

                case s.RESOLVED:
                return
                    filterAvailableStatus.Where(
                        l => l.Value.EqualsAny(s.CLOSED, s.CANCELLED, s.INPROG, s.QUEUED, s.REJECTED, s.RESOLVCONF)).ToList();

                case s.COMP:
                return filterAvailableStatus.Where(l => l.Value.EqualsAny(s.CLOSED, s.CANCELLED)).ToList();

                case s.PENDING:
                return
                    filterAvailableStatus.Where(
                        l =>
                            l.Value.EqualsAny(s.CANCELLED, s.INPROG, s.QUEUED, s.REJECTED, s.RESOLVCONF, s.RESOLVED,
                                s.SLAHOLD)).ToList();

                case s.SLAHOLD:
                return
                    filterAvailableStatus.Where(
                        l =>
                            l.Value.EqualsAny(s.CANCELLED, s.INPROG, s.QUEUED, s.REJECTED, s.RESOLVCONF, s.RESOLVED,
                                s.PENDING)).ToList();

            }
            return new List<IAssociationOption>();
        }
    }
}
