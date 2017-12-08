using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services {
    public class DispatchEscalationService : ASwJob {


        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public DispatchEmailCompositeService EmailService { get; set; }

        [Import] public IMemoryContextLookuper ContextLookuper { get; set; }


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
            if (!ApplicationConfiguration.IsClient("firstsolardispatch")) {
                //to prevent local environments to dispatch emails while running other customers
                return;
            }

            var tickets = Dao.FindByQuery<DispatchTicket>(DispatchTicket.EscalationQuery, DateTime.Now);
            foreach (var ticket in tickets) {
                if (!ContextLookuper.GetFromMemoryContext<bool>(ticket.EmailMemoryKey())) {
                    //to prevent email to be triggered at the same time it is scheduled
                    await EmailService.SendEmails(ticket);
                }

            }

        }




        public override bool RunAtStartup() {
            return false;
        }
    }
}
