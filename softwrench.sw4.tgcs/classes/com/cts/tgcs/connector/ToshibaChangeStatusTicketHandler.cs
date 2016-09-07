using System;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.WS.Internal;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.connector {
    public class ToshibaChangeStatusTicketHandler : ChangeStatusTicketHandler {

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
