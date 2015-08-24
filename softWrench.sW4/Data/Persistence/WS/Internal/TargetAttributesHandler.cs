using System.Linq;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Internal {
    internal class TargetAttributesHandler {

        public static T SetValuesFromJSON<T>(T integrationObject, EntityMetadata metadata, CrudOperationData operationData) {
            return HandleAttributesValues(integrationObject, metadata, operationData);
        }

        private static T HandleAttributesValues<T>(T integrationObject, EntityMetadata metadata,
            CrudOperationData operationData) {
            if (metadata.Targetschema == null) {
                return integrationObject;
            }
            foreach (var attribute in metadata.Targetschema.TargetAttributes) {
                var name = attribute.Name;
                var attributeValue = operationData.GetAttribute(name);
                if (attributeValue == null) {
                    continue;
                }
                var key = attribute.TargetPath;
                var splitted = key.Split('/');
                object objectToUse = integrationObject;
                for (var i = 0; i < splitted.Count(); i++) {
                    var propertyName = splitted[i];
                    if (i != splitted.Count() - 1) {
                        objectToUse = ReflectionUtil.InstantiateAndSetIfNull(objectToUse, propertyName);
                    } else {
                        if (ApplicationConfiguration.IsMif() || ApplicationConfiguration.IsMea()) {
                            WsUtil.SetValue(objectToUse, propertyName, attributeValue);
                        } else {

                            ReflectionUtil.SetProperty(objectToUse, propertyName, attributeValue);
                        }
                    }
                }
            }
            return integrationObject;
        }
    }
}
