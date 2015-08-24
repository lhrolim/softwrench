using System.Collections.Generic;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Util;
using WsUtil = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using softWrench.sW4.wsWorkorder;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class EmailAddressHandler {
        public static void HandleEmailAddress(CrudOperationData entity, object rootObject) {

            if (entity.GetAttribute("#primaryemail")!=null) {
                //new users, let´s generate adapt to a composition email to store it in maximo
                var arr = ReflectionUtil.InstantiateArrayWithBlankElements(rootObject, "EMAIL", 1);
                var email = arr.GetValue(0);
                ReflectionUtil.SetProperty(email, "action", ProcessingActionType.AddChange.ToString());
                WsUtil.SetValue(email, "isprimary", true, true);
                WsUtil.SetValue(email, "EMAILADDRESS", entity.GetAttribute("#primaryemail"), true);
                WsUtil.SetValue(email, "type", "WORK", true);
                return;
            }


            var emailAddress = ((IEnumerable<CrudOperationData>)entity.GetRelationship("email"));
            WsUtil.CloneArray(emailAddress, rootObject, "EMAIL", delegate (object integrationObject, CrudOperationData crudData) {
                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.AddChange.ToString());
                ReflectionUtil.InstantiateAndSetIfNull(integrationObject, "type");
            });
        }
    }
}

