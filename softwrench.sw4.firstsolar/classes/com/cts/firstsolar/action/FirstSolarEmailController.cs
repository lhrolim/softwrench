using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Data.Persistence.SWDB;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.callout.exception;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email;
using softwrench.sw4.webcommons.classes.api;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action {

    [NoMenuController]
    public class FirstSolarEmailController : Controller {
        [Import]
        public SWDBHibernateDAO DAO { get; set; }

        [Import]
        public FirstSolarCallOutEmailService FirstSolarCallOutEmailService { get; set; }
        

        [Import]
        public FirstSolarMaintenanceEmailService FirstSolarMaintenanceEmailService { get; set; }


        [System.Web.Http.HttpGet]
        public async Task<ActionResult> TransitionCallout(string token, string status) {
            return await DoTransition<CallOut>(token, status, FirstSolarCallOutEmailService);
        }

        [System.Web.Http.HttpGet]
        public async Task<ActionResult> TransitionMaintenanceEngineering(string token, string status) {
            return await DoTransition<MaintenanceEngineering>(token, status, FirstSolarMaintenanceEmailService);
        }


        [System.Web.Http.HttpGet]
        private async Task<ActionResult> DoTransition<T>(string token, string status, FirstSolarBaseEmailService emailService) where T : class, IFsEmailRequest, new() {
            var entity = await DAO.FindSingleByQueryAsync<T>(new T().ByToken, token);
            if (entity == null) {
                throw IFSEmailWorkflowException.NotFound<T>();
            }

            if (entity.Status.Equals(RequestStatus.Approved) || entity.Status.Equals(RequestStatus.Rejected)) {
                throw IFSEmailWorkflowException.AlreadyApprovedRejected<T>(entity.Id, entity.Status);
            }

            var newStatus = RequestStatus.Sent;
            Enum.TryParse(status, true, out newStatus);

            entity.Status = newStatus;

            await DAO.SaveAsync(entity);
            var wp = await DAO.FindByPKAsync<WorkPackage>(entity.WorkPackageId);

            if (RequestStatus.Rejected.Equals(newStatus)) {
                emailService.HandleReject(entity, wp);
            }

            return View("GenericRequest", new EmailRequestModel { WoNum = wp.Wpnum, Token = token, Type = entity.GetType().Name, Action = newStatus.LabelName(), EntityName = entity.EntityDescription });
        }



    }

    public class EmailRequestModel : IBaseLayoutModel {
        public string WoNum { get; set; }
        public string Action { get; set; }
        public string Token { get; set; }

        public string Type { get; set; }

        public string Title { get; set; }
        public string ClientName {
            get { return "firstsolar"; }
            set { }
        }

        public string EntityName { get; set; }
    }



    public class FirstSolarEmailRestController : ApiController {

        [Import]
        public SWDBHibernateDAO DAO { get; set; }


        [System.Web.Http.HttpPut]
        public async Task AddWorkLogToCallOut(string token, string worklog) {
            await DoAddWorkLog<CallOut>(token, worklog);
        }

        [System.Web.Http.HttpPut]
        public async Task AddWorkLogToMaintenanceEngineering(string token, string worklog) {
            await DoAddWorkLog<MaintenanceEngineering>(token, worklog);
        }

        [System.Web.Http.HttpPut]
        private async Task DoAddWorkLog<T>(string token, string worklog) where T : class, IFsEmailRequest, new() {
            var entity = await DAO.FindSingleByQueryAsync<T>(new T().ByToken, token);
            if (entity == null) {
                throw IFSEmailWorkflowException.NotFound<T>();
            }
            entity.Notes = worklog;
            await DAO.SaveAsync(entity);
        }

    }
}
