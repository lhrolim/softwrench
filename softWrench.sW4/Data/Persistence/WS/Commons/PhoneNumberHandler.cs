using System;
using System.Collections.Generic;
using System.Linq;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using WsUtil = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using softWrench.sW4.wsWorkorder;

namespace softWrench.sW4.Data.Persistence.WS.Commons
{
    class PhoneNumberHandler
    {
        public static void HandlePhoneNumbers(CrudOperationData entity, object rootObject)
        {
            var Phones = ((IEnumerable<CrudOperationData>) entity.GetRelationship("phone"));
            WsUtil.CloneArray(Phones, rootObject, "PHONE", delegate(object integrationObject, CrudOperationData crudData) {

                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.AddChange.ToString());
            });
        }
    }
}

