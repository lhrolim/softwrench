using System;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Util;
using WebGrease.Css.Extensions;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.jobs {
    public class FirstSolarWorkPackageMaintenanceEngineeringMailJob : ASwJob {
        private readonly ISWDBHibernateDAO _dao;
        private readonly IMaximoHibernateDAO _maximoDao;
        private readonly FirstSolarMaintenanceEngineeringHandler _meHandler;

        public FirstSolarWorkPackageMaintenanceEngineeringMailJob(ISWDBHibernateDAO dao, IMaximoHibernateDAO maximoDao, FirstSolarMaintenanceEngineeringHandler meHandler) {
            _dao = dao;
            _maximoDao = maximoDao;
            _meHandler = meHandler;
        }

        public override string Name() {
            return "First Solar Work Package Maintenance Engineering Emails";
        }

        public override string Description() {
            return "Send work package emails of maintenance engineering request.";
        }

        public override string Cron() {
            if (ApplicationConfiguration.IsLocal()) {
                return "30 * * * * ?";
            }
            return "0 2 * * * ?";
        }

        public override bool RunAtStartup() {
            return true;
        }

        public override async Task ExecuteJob() {
            var mes = await _dao.FindByQueryAsync<MaintenanceEngineering>(MaintenanceEngineering.ByStatusAndTime, DateTime.Now);
            if (mes != null && mes.Any()) {
                mes.ForEach(HandleMaintenanceEngineering);
                Log.InfoFormat("done sending {0} maintenance engineerings", mes.Count);
            } else {
                Log.InfoFormat("no maintenance engineering sent");
            }
        }

        private void HandleMaintenanceEngineering(MaintenanceEngineering me) {
            var package = me.WorkPackage;
            var wos = _maximoDao.FindByNativeQuery("select siteid from workorder where workorderid = '{0}'".Fmt(package.WorkorderId));
            var siteid = wos.First()["siteid"];
            _meHandler.HandleEmail(me, package, siteid);
        }
    }
}
