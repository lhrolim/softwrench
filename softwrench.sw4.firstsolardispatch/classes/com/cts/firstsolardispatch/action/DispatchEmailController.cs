using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Web.Mvc;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.dataset;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services;
using softwrench.sw4.webcommons.classes.api;
using softwrench.sW4.audit.classes.Model;
using softwrench.sW4.audit.classes.Services;
using softWrench.sW4.Data.Persistence.SWDB;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.action {
    [NoMenuController]
    public class DispatchEmailController : Controller {
        [Import]
        public SWDBHibernateDAO DAO { get; set; }

        [Import]
        public AuditManager AuditManager { get; set; }

        [Import]
        public DispatchStatusService StatusService { get; set; }

        [System.Web.Http.HttpGet]
        public async Task<ActionResult> ChangeStatus(string token, string status) {
            var ticket = await DAO.FindSingleByQueryAsync<DispatchTicket>(DispatchTicket.ByToken, token);
            if (ticket == null) {
                throw new Exception("Ticket not found. Please contact support");
            }

            var oldStatus = ticket.Status;
            DispatchTicketStatus newStatus;
            Enum.TryParse(status, true, out newStatus);

            await StatusService.ValidateStatusChange(oldStatus, newStatus, ticket, false);

            ticket.Status = newStatus;
            await DAO.SaveAsync(ticket);

            if (oldStatus != newStatus) {
                var entry = new AuditEntry(FirstSolarDispatchTicketDataSet.StatusAuditAction, "_dispatchticket",
                    ticket.Id.ToString(), ticket.Id.ToString(), ticket.Status.LabelName(), "", DateTime.Now);
                AuditManager.SaveAuditEntry(entry);
            }

            return View("GenericRequest", new EmailRequestModel { Id = ticket.Id, Status = ticket.Status.LabelName() });
        }
    }

    public class EmailRequestModel : IBaseLayoutModel {
        public int? Id { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string ClientName {
            get { return "firstsolardispatch"; }
            set { }
        }
    }
}
