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

        protected override TargetResult DoExecute(NewStatusData crudOperationData, MaximoOperationExecutionContext maximoExecutionContext) {
            var toshibaData = (ToshibaStatusData)crudOperationData;
            try {
                return base.DoExecute(crudOperationData, maximoExecutionContext);
            } catch (Exception e) {
                if (toshibaData.JobMode) {
                    var problem = Problem.BaseProblem(crudOperationData.ApplicationMetadata.Name, "editdetail", crudOperationData.CrudData.Id, crudOperationData.CrudData.UserId, e.StackTrace, e.Message, "ism.sr.statussync");
                    _problemManager.RegisterOrUpdateProblem(SecurityFacade.CurrentUser().UserId.Value, problem, null);
                    return null;
                }
                throw;
            }
        }

        public class ToshibaStatusData : NewStatusData {
            public DateTime? CloseDate {
                get; set;
            }

            /// <summary>
            /// In order to diferentiate operations that are runing on the sync job, and would require problems to be openend
            /// </summary>
            public bool JobMode {
                get; set;
            }
        }

    }
}
