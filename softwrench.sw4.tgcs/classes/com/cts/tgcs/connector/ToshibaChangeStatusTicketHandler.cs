using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using softwrench.sw4.problem.classes;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.connector {
    public class ToshibaChangeStatusTicketHandler : ChangeStatusTicketHandler {

        private readonly IProblemManager _problemManager;

        public ToshibaChangeStatusTicketHandler() {
            _problemManager = SimpleInjectorGenericFactory.Instance.GetObject<IProblemManager>(typeof(IProblemManager));
        }

        protected override MaximoOperationExecutionContext PrepareData(NewStatusData crudOperationData) {
            var executionContext = base.PrepareData(crudOperationData);
            var ticket = executionContext.IntegrationObject;
            var toshibaData = (ToshibaStatusData)crudOperationData;
            if (toshibaData.CloseDate != null) {
                WsUtil.SetValue(ticket, "itdclosedate", toshibaData.CloseDate);
            }
            return executionContext;
        }


        public class ToshibaStatusData : NewStatusData {
            public DateTime? CloseDate {
                get; set;
            }

      
            public bool JobMode {
                get; set;
            }
        }

    }
}
