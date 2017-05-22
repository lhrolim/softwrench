﻿using System;
using System.Threading.Tasks;
using cts.commons.persistence;
using log4net;
using NHibernate.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Scheduler;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.jobs {
    public class FirstSolarWorkPackageMaintenanceEngineeringMailJob : ASwJob {
        private const string MaintenanceEngQuery = "from MaintenanceEngineering where status = ? and sendTime <= ?";
        private readonly ISWDBHibernateDAO _dao;
        private readonly ILog _log = LogManager.GetLogger(typeof(FirstSolarWorkPackageMaintenanceEngineeringMailJob));

        public FirstSolarWorkPackageMaintenanceEngineeringMailJob(ISWDBHibernateDAO dao) {
            _dao = dao;
        }

        public override string Name() {
            return "First Solar Work Package Maintenance Engineering Emails";
        }

        public override string Description() {
            return "Send work package emails of maintenance engineering request.";
        }

        public override string Cron() {
            return "0 2 * * * ?";
        }

        public override bool RunAtStartup() {
            return true;
        }

        public override async Task ExecuteJob() {
            var mes = _dao.FindByQuery<MaintenanceEngineering>(MaintenanceEngQuery, FSWPackageConstants.MaintenanceEngStatus.Pending, DateTime.Now);
            if (mes != null && mes.Any()) {
                mes.ForEach(HandleMaintenanceEngineering);
            }
        }

        private void HandleMaintenanceEngineering(MaintenanceEngineering me) {
            // TODO send the email
            me.Status = FSWPackageConstants.CallOutStatus.Submited;
            _dao.Save(me);
        }
    }
}
