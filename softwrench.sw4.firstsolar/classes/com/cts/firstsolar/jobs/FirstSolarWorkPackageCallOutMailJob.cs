using System;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using log4net;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email;
using softWrench.sW4.Scheduler;
using WebGrease.Css.Extensions;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.jobs {
    public class FirstSolarWorkPackageCallOutMailJob : ASwJob {
        private const string CallOutQuery = "from CallOut where status = ? and sendTime <= ?";
        private readonly ISWDBHibernateDAO _dao;
        private readonly FirstSolarCallOutEmailService _callOutEmailService;
        private readonly ILog _log = LogManager.GetLogger(typeof(FirstSolarWorkPackageCallOutMailJob));

        public FirstSolarWorkPackageCallOutMailJob(ISWDBHibernateDAO dao, FirstSolarCallOutEmailService callOutEmailService) {
            _dao = dao;
            _callOutEmailService = callOutEmailService;
        }

        public override string Name() {
            return "First Solar Work Package Call Out Emails";
        }

        public override string Description() {
            return "Send work package emails of subcontractor call outs.";
        }

        public override string Cron() {
            return "0 1 * * * ?";
        }

        public override bool RunAtStartup() {
            return true;
        }

        public override async Task ExecuteJob(){
            var callOuts = _dao.FindByQuery<CallOut>(CallOutQuery, FSWPackageConstants.CallOutStatus.Completed, DateTime.Now);
            if (callOuts != null && callOuts.Any()) {
                callOuts.ForEach(HandleCallout);
            }
        }

        private void HandleCallout(CallOut callOut) {
            _callOutEmailService.SendCallout(callOut, callOut.Email);
            callOut.Status = FSWPackageConstants.CallOutStatus.Submited;
            _dao.Save(callOut);
        }
    }
}
