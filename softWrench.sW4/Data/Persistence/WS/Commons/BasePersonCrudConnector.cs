using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Email;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    class BasePersonCrudConnector : CrudConnectorDecorator {

        private readonly PhoneNumberHandler _phoneHandler;
        private readonly MaximoConnectorEngine _maximoConnectorEngine;

        public BasePersonCrudConnector() {
            _phoneHandler = SimpleInjectorGenericFactory.Instance.GetObject<PhoneNumberHandler>(typeof(PhoneNumberHandler));
            _maximoConnectorEngine = SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(typeof(MaximoConnectorEngine));
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var person = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);

            if (ApplicationConfiguration.Profile.EqualsIc("demo")) {
                crudData.UnmappedAttributes["#password"] = "";
                crudData.UnmappedAttributes["#retypepassword"] = "";
            }

            var emailToDelete = EmailAddressHandler.HandleEmailAddress(crudData, person);
            if (emailToDelete != null) {
                maximoTemplateData.Properties.Add("emailtodelete", emailToDelete);
            }

            var phoneToDelete = _phoneHandler.HandlePhoneNumbers(crudData, person);
            if (phoneToDelete != null) {
                maximoTemplateData.Properties.Add("phonetodelete", phoneToDelete);
            }


            base.BeforeUpdate(maximoTemplateData);
        }

        public override void AfterUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var props = maximoTemplateData.Properties;
            if (props.ContainsKey("emailtodelete")) {
                var crudEmail = props["emailtodelete"];
                _maximoConnectorEngine.Delete((CrudOperationData)crudEmail);
            }
            if (props.ContainsKey("phonetodelete")) {
                var crudPhone = props["phonetodelete"];
                _maximoConnectorEngine.Delete((CrudOperationData)crudPhone);
            }
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var person = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);

            EmailAddressHandler.HandleEmailAddress(crudData, person);
            _phoneHandler.HandlePhoneNumbers(crudData, person);

            base.BeforeCreation(maximoTemplateData);
        }
    }
}
