using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Text;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using System.Net.Mail;
using System.Net;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Email;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    class BasePhysicalCountCrudConnector : CrudConnectorDecorator {

        public BasePhysicalCountCrudConnector() {
            
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var physicalCount = maximoTemplateData.IntegrationObject;
            w.SetValue(physicalCount, "reconciled", false);
            base.BeforeCreation(maximoTemplateData);
        }
    }
}
