using softWrench.sW4.Data.Persistence.Operation;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {
    public class LongDescriptionHandler {

        public static T HandleLongDescription<T>(T integrationObject, CrudOperationData entity, string property = "DESCRIPTION_LONGDESCRIPTION", string relatedobject = "longdescription") {
            var ld = (CrudOperationData)entity.GetRelationship(relatedobject,true);
            if (ld != null) {
                w.SetValue(integrationObject, property, ld.GetAttribute("ldtext"));
            } else {
                var longDescription = entity.GetAttribute("ld_.ldtext") ?? entity.GetAttribute("wld_.ldtext");
                w.SetValue(integrationObject, property, longDescription);
            }

            return integrationObject;
        }
    }
}
