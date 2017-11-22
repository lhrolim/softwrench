using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using Newtonsoft.Json.Linq;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.handlers;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services;
using softwrench.sW4.audit.Interfaces;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.dataset {
    public class FirstSolarDispatchTicketDataSet : SWDBApplicationDataset {
        public const string StatusAuditAction = "dispatcher_status";

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public FirstSolarInverterHandler InverterHandler { get; set; }

        [Import]
        public IAuditManager AuditManager { get; set; }

        [Import]
        public DispatchSchedullerService SchedullerService { get; set; }

        [Import]
        public DispatchEmailService DispatchEmailService { get; set; }

        [Import]
        public DispatchStatusService StatusService { get; set; }


        protected override async Task<DataMap> FetchDetailDataMap(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var baseData = await base.FetchDetailDataMap(application, user, request);
            baseData.SetAttribute("#originalstatus", baseData.GetAttribute("status"));
            if (baseData.ContainsAttribute("immediatedispatch",true))
            {
                baseData.SetAttribute("immediatedispatch", baseData.GetAttribute("immediatedispatch").ToString().ToLower());
            }
            

            return baseData;
        }

        public override async Task<CompositionFetchResult> GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request, JObject currentData) {
            var compData = await base.GetCompositionData(application, request, currentData);


            if (request.CompositionList != null && !request.CompositionList.Contains("#statushistory_")) {
                return compData;
            }

            var entries = await AuditManager.Lookup(ApplicationName(), request.Id, StatusAuditAction);
            var statusResult = new EntityRepository.SearchEntityResult();
            var totalCount = entries.Count();



            statusResult.PaginationData = new PaginatedSearchRequestDto(totalCount, 1, totalCount, null, new List<int>() { totalCount });
            statusResult.ResultList = new List<Dictionary<string, object>>();

            foreach (var entry in entries) {
                var dict = new Dictionary<string, object>();
                dict.Add("changeby", entry.CreatedBy);
                dict.Add("changedate", entry.CreatedDate);
                dict.Add("status", entry.DataStringValue);
                statusResult.ResultList.Add(dict);
            }

            compData.ResultObject.Add("#statushistory_", statusResult);
            return compData;
        }


        [Transactional(DBType.Swdb)]
        public override async Task<TargetResult> DoExecute(OperationWrapper operationWrapper) {
            var crudoperationData = (CrudOperationData)operationWrapper.OperationData();
            var isCreation = OperationConstants.CRUD_CREATE.Equals(operationWrapper.OperationName);
            var dispatchingBase = crudoperationData.GetBooleanAttribute("#dispatching");
            var dispatching = (dispatchingBase != null && dispatchingBase.Value);

            var ticket = GetOrCreate<DispatchTicket>(operationWrapper, false);
            var oldStatus = ticket.Status;
            EntityBuilder.PopulateTypedEntity((CrudOperationData) operationWrapper.OperationData(), ticket);

            if (isCreation || dispatching) {
                ticket.Status = dispatching ? ticket.ImmediateDispatch ? DispatchTicketStatus.DISPATCHED : DispatchTicketStatus.SCHEDULED : DispatchTicketStatus.DRAFT;
            }
            var hasStatusChange = oldStatus != ticket.Status || isCreation;
            await StatusService.ValidateStatusChange(oldStatus, ticket.Status, ticket);

            if (ticket.AccessToken == null) {
                ticket.AccessToken = TokenUtil.GenerateDateTimeToken();
            }

            var user = SecurityFacade.CurrentUser().DBUser;
            ticket.ReportedBy = user;

            if (ticket.ImmediateDispatch && dispatching) {
                ticket.DispatchExpectedDate = DateTime.Now;
            }

            if (hasStatusChange) {
                ticket.StatusReportedBy = user;
                ticket.StatusDate = DateTime.Now;
            }

            ticket.GpsLatitude = ConvertDecimal(crudoperationData, "gpslatitude");
            ticket.GpsLongitude = ConvertDecimal(crudoperationData, "gpslongitude");

            if (ticket.CreatedDate == null) {
                ticket.CreatedDate = DateTime.Now;
            }
            //saving the ticket first in order to obtain a valid id
            ticket = await Dao.SaveAsync(ticket);
            if (hasStatusChange) {
                AuditManager.CreateAuditEntry(StatusAuditAction, ApplicationName(), ticket.Id.ToString(), ticket.Id.ToString(), ticket.Status.LabelName(), DateTime.Now);
            }


            var newTickets = HandleFileExplorerDocLinks(ticket, crudoperationData, "attachments_");
            foreach (var newTicket in newTickets) {
                ticket.Attachments.Add(newTicket);
            }
            InverterHandler.HandleInverters(crudoperationData, ticket, operationWrapper.ApplicationMetadata.Schema);

            ticket = await Dao.SaveAsync(ticket);

            var targetResult = new TargetResult(ticket.Id.ToString(), null, DataMap.BlankInstance("_dispatchticket")) {
                ReloadMode = ReloadMode.MainDetail
            };

            if (!hasStatusChange || !dispatching) return targetResult;

            if (ticket.ImmediateDispatch) {
                var site = await Dao.FindSingleByQueryAsync<GfedSite>(GfedSite.FromGFedId, ticket.GfedId);
                DispatchEmailService.SendEmail(ticket, site);
                //SchedullerService.ScheduleDispatch(ticket);
            } else {
                //SchedullerService.ScheduleDispatch(ticket);
            }

            return targetResult;
        }

        //        public IEnumerable<IAssociationOption> FilterAvailableStatus(AssociationPostFilterFunctionParameters postFilter){
        //            
        //        }

        private decimal? ConvertDecimal(CrudOperationData data, string att) {
            var attStringValue = data.GetStringAttribute(att);
            if (string.IsNullOrEmpty(attStringValue)) {
                return null;
            }
            return Convert.ToDecimal(attStringValue);
        }

        public override string ApplicationName() {
            return "_dispatchticket";
        }
    }
}
