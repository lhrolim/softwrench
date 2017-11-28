using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email {
    public class DispatchEmailCompositeService : ISingletonComponent {

        [Import]
        public DispatchEmailService DispatchEmailService { get; set; }

        [Import]
        public DispatchSmsEmailService DispatchSmsEmailService { get; set; }

        public async Task SendEmails(DispatchTicket ticket) {
            await DispatchEmailService.SendEmail(ticket);
//            await DispatchSmsEmailService.SendEmail(ticket, hour);
        }

    }
}
