﻿using System;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Util;
using WebGrease.Css.Extensions;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.jobs {
    public class FirstSolarWorkPackageCallOutMailJob : ASwJob {

        private readonly ISWDBHibernateDAO _dao;
        private readonly IMaximoHibernateDAO _maximoDao;
        private readonly FirstSolarCallOutHandler _callOutHandler;
        private readonly FirstSolarCustomGlobalFedService _gefedService;

        public FirstSolarWorkPackageCallOutMailJob(ISWDBHibernateDAO dao, IMaximoHibernateDAO maximoDao, FirstSolarCallOutHandler callOutHandler, FirstSolarCustomGlobalFedService gefedService) {
            _dao = dao;
            _maximoDao = maximoDao;
            _callOutHandler = callOutHandler;
            _gefedService = gefedService;
        }

        public override string Name() {
            return "First Solar Work Package Call Out Emails";
        }

        public override string Description() {
            return "Send work package emails of subcontractor call outs.";
        }

        public override string Cron() {
            if (ApplicationConfiguration.IsLocal()) {
                return "30 * * * * ?";
            }
            return "0 1 * * * ?";
        }

        public override bool RunAtStartup() {
            //requires memory context for the iis to be up
            return false;
        }

        public override async Task ExecuteJob() {
            var callOuts = await _dao.FindByQueryAsync<CallOut>(CallOut.ByStatusAndTime, DateTime.Now);
            if (callOuts != null && callOuts.Any()) {
                callOuts.ForEach(HandleCallout);
                Log.InfoFormat("done sending {0} callouts", callOuts.Count);
            } else {
                Log.InfoFormat("no callouts sent");
            }
        }

        private void HandleCallout(CallOut callOut){
            var package = callOut.WorkPackage;
            AsyncHelper.RunSync(() => _gefedService.LoadGfedData(package, callOut));
            var wos = _maximoDao.FindByNativeQuery("select siteid, worktype from workorder where workorderid = '{0}'".Fmt(package.WorkorderId));
            var woData = new WorkOrderData {
                SiteId = wos.First()["siteid"],
                WorkType = wos.First()["worktype"]
            };
        
            _callOutHandler.HandleEmail(callOut, package, woData);
        }
    }
}
