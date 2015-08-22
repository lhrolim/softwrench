using System.Collections.Generic;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Util;
using WsUtil = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using softWrench.sW4.wsWorkorder;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class PhoneNumberHandler {

        public static void HandlePhoneNumbers(CrudOperationData entity, object rootObject) {

            if (entity.GetAttribute("#primaryphone")!=null) {
                //new users, let´s generate adapt to a composition email to store it in maximo
                var arr = ReflectionUtil.InstantiateArrayWithBlankElements(rootObject, "PHONE", 1);
                var email = arr.GetValue(0);
                ReflectionUtil.SetProperty(email, "action", ProcessingActionType.AddChange.ToString());
                WsUtil.SetValue(email, "isprimary", true,true);
                WsUtil.SetValue(email, "phonenum", entity.GetAttribute("#primaryphone"), true);
                WsUtil.SetValue(email, "type", "WORK", true);
                return;
            }

            var phones = ((IEnumerable<CrudOperationData>)entity.GetRelationship("phone"));
            WsUtil.CloneArray(phones, rootObject, "PHONE", delegate (object integrationObject, CrudOperationData crudData) {

                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.AddChange.ToString());
                ReflectionUtil.InstantiateAndSetIfNull(integrationObject, "type");
            });
        }
    }
}

