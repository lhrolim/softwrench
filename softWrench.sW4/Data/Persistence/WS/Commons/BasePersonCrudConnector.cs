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
using System.Linq;
using System.Net.Mail;
using System.Net;
using softWrench.sW4.Configuration.Services.Api;
using cts.commons.simpleinjector;
using softWrench.sW4.Email;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Maximo;


namespace softWrench.sW4.Data.Persistence.WS.Commons {

    class BasePersonCrudConnector : CrudConnectorDecorator {

        public BasePersonCrudConnector() {
            
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var person = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);

            EmailAddressHandler.HandleEmailAddress(crudData, person);
            PhoneNumberHandler.HandlePhoneNumbers(crudData, person);

            base.BeforeUpdate(maximoTemplateData);
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var person = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
        }
    }
}
