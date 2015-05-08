using softWrench.sW4.Data.Persistence.Dataset.Commons.Maximo;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class BaseSolutionCrudConnector : CrudConnectorDecorator {
        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            CommonTransaction(maximoTemplateData);
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            CommonTransaction(maximoTemplateData);
        }

        public void CommonTransaction(MaximoOperationExecutionContext maximoTemplateData) {
            LongDescriptionHandler.HandleLongDescription(maximoTemplateData.IntegrationObject, (CrudOperationData)maximoTemplateData.OperationData, "PROBLEMCODE_LONGDESCRIPTION", "symptom");
            LongDescriptionHandler.HandleLongDescription(maximoTemplateData.IntegrationObject, (CrudOperationData)maximoTemplateData.OperationData, "FR1CODE_LONGDESCRIPTION", "cause");
            LongDescriptionHandler.HandleLongDescription(maximoTemplateData.IntegrationObject, (CrudOperationData)maximoTemplateData.OperationData, "FR2CODE_LONGDESCRIPTION", "resolution");
        }
    }
}
