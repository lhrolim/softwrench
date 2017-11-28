using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email;
using softWrench.sW4.Scheduler;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services {
    public class DispatchEscalationService : ASwJob {


        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public DispatchEmailCompositeService EmailService { get; set; }


        public override string Name() {
            return "Dispatcher Escalation";
        }

        public override string Description() {
            return "Handles Dispatch escalation";
        }

        public override string Cron() {
            return "0 */1 * ? * *";
        }

        public override async Task ExecuteJob() {
            var tickets = Dao.FindByQuery<DispatchTicket>(DispatchTicket.EscalationQuery);
            foreach (var ticket in tickets) {
                await EmailService.SendEmails(ticket);
            }

        }

       


        public override bool RunAtStartup() {
            return false;
        }
    }
}
