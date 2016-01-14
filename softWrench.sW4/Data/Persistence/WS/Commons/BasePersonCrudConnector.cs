﻿using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

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

            EmailAddressHandler.HandleEmailAddress(crudData, person);
            PhoneNumberHandler.HandlePhoneNumbers(crudData, person);

            base.BeforeCreation(maximoTemplateData);
        }
    }
}