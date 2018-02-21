using System;
using System.ComponentModel.Composition;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using cts.commons.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.config;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.dataset;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email;
using softwrench.sw4.webcommons.classes.api;
using softwrench.sW4.audit.classes.Model;
using softwrench.sW4.audit.classes.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.action {
    [NoMenuController]
    public class DispatchEmailController : Controller {
        [Import]
        public SWDBHibernateDAO DAO { get; set; }

        [Import]
        public AuditManager AuditManager { get; set; }

        [Import]
        public DispatchStatusService StatusService { get; set; }

        [Import]
        public DispatchAcceptedEmailService DispatchAcceptedEmailService { get; set; }

        [Import]
        public DispatchArrivedEmailService DispatchArrivedEmailService { get; set; }

        [Import]
        public IConfigurationFacade ConfigurationFacade { get; set; }


        [System.Web.Http.HttpGet]
        public async Task<ActionResult> Ac(string token, string status) {
            return await ChangeStatus(token, "ACCEPTED");
        }

        public async Task<ActionResult> Rj(string token, string status) {
            return await ChangeStatus(token, "REJECTED");
        }

        [System.Web.Http.HttpGet]
        public async Task<ActionResult> ChangeStatus(string token, string status) {
            var ticket = await DAO.FindSingleByQueryAsync<DispatchTicket>(DispatchTicket.ByToken, token);
            if (ticket == null) {
                throw new Exception("Ticket not found. Please contact support");
            }

            var oldStatus = ticket.Status;
            DispatchTicketStatus newStatus;
            Enum.TryParse(status, true, out newStatus);

            StatusService.ValidateStatusChange(oldStatus, newStatus, ticket, ApplicationConfiguration.IsLocal());

            ticket.Status = newStatus;
            await DAO.SaveAsync(ticket);

            if (oldStatus != newStatus) {
                var entry = new AuditEntry(FirstSolarDispatchTicketDataSet.StatusAuditAction, "_dispatchticket",
                    ticket.Id.ToString(), ticket.Id.ToString(), ticket.Status.LabelName(), "", DateTime.Now);
                AuditManager.SaveAuditEntry(entry);
                if (DispatchTicketStatus.ACCEPTED.Equals(newStatus)) {
                    await DispatchAcceptedEmailService.SendEmail(ticket);
                }
                if (DispatchTicketStatus.ARRIVED.Equals(newStatus)) {
                    await DispatchArrivedEmailService.SendEmail(ticket);
                }
            }

            if (ticket.Status.Equals(DispatchTicketStatus.ACCEPTED)) {
                return View("Accepted", await BuildAcceptedModel(ticket));
            }

            return View("GenericRequest", new EmailRequestModel { Id = ticket.Id, Status = ticket.Status.LabelName() });
        }

        private async Task<AcceptedEmailRequestModel> BuildAcceptedModel(DispatchTicket ticket) {
            var hmacKey = await ConfigurationFacade.LookupAsync<string>(ConfigurationConstants.HashKey);
            var hashKey = AuthUtils.HmacShaEncode(ticket.Id.ToString(), Encoding.ASCII.GetBytes(hmacKey));
            var serverurl = await ConfigurationFacade.LookupAsync<string>(FirstSolarDispatchConfigurations.ProductionFsiisEndpoint);
            var gfedSite = await DAO.FindSingleByQueryAsync<GfedSite>(GfedSite.FromGFedId, ticket.GfedId);
            ticket.SiteId = gfedSite.SiteId;

            var jsonSerializerSettings = new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var ticketJSON = JsonConvert.SerializeObject(ticket, Newtonsoft.Json.Formatting.None, jsonSerializerSettings);


            return new AcceptedEmailRequestModel { Id = ticket.Id, Status = ticket.Status.LabelName(), HashKey = hashKey, ServerUrl = serverurl, TicketJSON = ticketJSON };
        }
    }

    public class EmailRequestModel : ABaseLayoutModel {
        public int? Id { get; set; }
        public string Status { get; set; }
        
        public override string ClientName {
            get { return "firstsolardispatch"; }
            set { }
        }

        public override bool PreventPoweredBy => true;

    }

    public class AcceptedEmailRequestModel : EmailRequestModel {

        public string HashKey { get; set; }

        public string ServerUrl { get; set; }

        public string TicketJSON { get; set; }

    }
}
