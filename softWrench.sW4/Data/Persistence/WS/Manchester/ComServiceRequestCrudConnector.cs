using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Security.Services;
using softWrench.sW4.wsAsset;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Manchester
{
    class ComServiceRequestCrudConnector : BaseServiceRequestCrudConnector
    {
        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeUpdate(maximoTemplateData);
            
            var sr = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeCreation(maximoTemplateData);
            
            var sr = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
        }
    }
}
