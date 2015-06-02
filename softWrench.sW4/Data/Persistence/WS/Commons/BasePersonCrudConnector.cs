using System.Collections.Generic;
using System.Web.Script.Serialization;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using Iesi.Collections.Generic;
using softWrench.sW4.Security.Entities;
using Newtonsoft.Json;
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

            //HandleSWUser((CrudOperationData)maximoTemplateData.OperationData);

            base.BeforeUpdate(maximoTemplateData);
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var person = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
        }

        private void HandleSWUser(CrudOperationData crudData) {
            var username = crudData.GetAttribute("personid");
            var profiles = crudData.GetUnMappedAttribute("#profiles");
            
            User user = UserManager.GetUserByUsername(username.ToString());
            user.Profiles = profiles;
            SWDB.SWDBHibernateDAO.GetInstance().Save(user);
        }
    }
}
