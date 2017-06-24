using System.Linq;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Internal {
    internal class TargetConstantHandler {

        public static T SetConstantValues<T>(T integrationObject, EntityMetadata metadata) {
            if (metadata.Targetschema == null) {
                return integrationObject;
            }
            foreach (var constValue in metadata.Targetschema.ConstValues) {
                var key = constValue.Key;
                var splitted = key.Split('/');
                object objectToUse = integrationObject;
                for (var i = 0; i < splitted.Count(); i++) {
                    var propertyName = splitted[i];
                    if (i != splitted.Count() - 1) {
                        objectToUse = ReflectionUtil.InstantiateAndSetIfNull(objectToUse, propertyName);
                    } else {
                        WsUtil.SetValueIfNull(objectToUse, propertyName, ConversionUtil.ConvertFromMetadataType(constValue.Type, constValue.Value,false));
                        //                        ReflectionUtil.SetProperty(objectToUse, propertyName, constValue.Value);
                    }
                }
            }
            return integrationObject;
        }
    }
}
