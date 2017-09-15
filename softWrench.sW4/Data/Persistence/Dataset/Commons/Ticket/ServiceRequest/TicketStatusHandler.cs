using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.Util;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;
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

            var baseResult = new SortedSet<IAssociationOption>();
            if (currentOption != null && addcurrent) {
                baseResult.Add(currentOption);
            }


            MaxSrStatus statusEnum;
            Enum.TryParse(currentStatus, true, out statusEnum);

            baseResult.AddAll(DoFilterAvailableStatus(postFilter.OriginalEntity, statusEnum, filterAvailableStatus));

            return baseResult;
        }

        public virtual ISet<IAssociationOption> DoFilterAvailableStatus(AttributeHolder originalEntity, MaxSrStatus statusEnum, ISet<IAssociationOption> filterAvailableStatus) {

            var slaHoldStatus = HandleSlaHoldStatus(originalEntity, statusEnum);



            switch (statusEnum) {
                case s.NEW:

                return filterAvailableStatus.Where(l => l.Value.EqualsAny(s.CANCELLED, s.QUEUED, s.REJECTED)).ToHashSet();


                case s.QUEUED:

                return filterAvailableStatus.Where(l => l.Value.EqualsAny(s.INPROG, s.REJECTED, s.CANCELLED)).ToHashSet();

                case s.INPROG:
                return
                    filterAvailableStatus.Where(
                        l =>
                            l.Value.EqualsAny(s.CANCELLED, s.PENDING, s.QUEUED, s.REJECTED, s.RESOLVCONF, s.RESOLVED,
                                slaHoldStatus)).ToHashSet();

                case s.CLOSED:
                return filterAvailableStatus.Where(l => l.Value.EqualsAny(s.CANCELLED)).ToHashSet();

                case s.APPR:
                    return filterAvailableStatus.Where(l => l.Value.EqualsAny(s.COMP, s.CLOSED)).ToHashSet();

                case s.RESOLVED:
                return
                    filterAvailableStatus.Where(
                        l => l.Value.EqualsAny(s.CLOSED, s.CANCELLED, s.INPROG, s.QUEUED, s.REJECTED, s.RESOLVCONF)).ToHashSet();

                case s.COMP:
                return filterAvailableStatus.Where(l => l.Value.EqualsAny(s.CLOSED, s.CANCELLED)).ToHashSet();

                case s.PENDING:
                return
                    filterAvailableStatus.Where(
                        l =>
                            l.Value.EqualsAny(s.CANCELLED, s.INPROG, s.QUEUED, s.REJECTED, s.RESOLVCONF, s.RESOLVED,
                                slaHoldStatus)).ToHashSet();

                case s.SLAHOLD:



                return
                    filterAvailableStatus.Where(
                        l =>
                            l.Value.EqualsAny(s.CANCELLED, s.INPROG, s.QUEUED, s.REJECTED, s.RESOLVCONF, s.RESOLVED,
                                s.PENDING)).ToHashSet();

            }
            return filterAvailableStatus;
        }

        /// <summary>
        /// Implementing SWWEB-2916
        /// </summary>
        /// <param name="originalEntity"></param>
        /// <param name="statusEnum"></param>
        /// <returns></returns>
        protected virtual MaxSrStatus HandleSlaHoldStatus(AttributeHolder originalEntity, MaxSrStatus statusEnum) {
            var acumulatedHoldTime = originalEntity.GetBooleanAttribute("ACCUMULATESLAHOLDTIME");
            if (!acumulatedHoldTime.HasValue || acumulatedHoldTime.Value == false) {
                return s.FAKE;
            }
            if (statusEnum.Equals(MaxSrStatus.QUEUED)) {
                return s.SLAHOLD;
            }

            if (originalEntity.GetStringAttribute("TARGETSTART") != null && originalEntity.GetStringAttribute("ACTUALSTART") == null) {
                return s.FAKE;
            }
            return s.SLAHOLD;
        }
    }
}
