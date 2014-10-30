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

    class BaseInvuseCrudConnector : CrudConnectorDecorator {

        public BaseInvuseCrudConnector() {
            
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var entity = (CrudOperationData)maximoTemplateData.OperationData;
            var invuse = maximoTemplateData.IntegrationObject;
            w.SetValueIfNull(invuse, "USETYPE", "TRANSFER");
            w.SetValue(invuse, "STATUS", "COMPLETE");
            var inventory = (CrudOperationData)entity.GetRelationship("inventory");
            InvuselineHandler.HandleInvuseline(entity, invuse);
            base.BeforeCreation(maximoTemplateData);
        }
    }
}
