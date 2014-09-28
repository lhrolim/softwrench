using System;
using System.Linq;
using softWrench.sW4.Data.Persistence.Operation;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class LongDescriptionHandler {

        public static T HandleLongDescription<T>(T integrationObject, CrudOperationData entity, string property = "DESCRIPTION_LONGDESCRIPTION") {
            var ld = (CrudOperationData)entity.GetRelationship("longdescription");
            if (ld != null) {
                w.SetValue(integrationObject, property, ld.GetAttribute("ldtext"));
            }
            return integrationObject;
        }
    }
}
