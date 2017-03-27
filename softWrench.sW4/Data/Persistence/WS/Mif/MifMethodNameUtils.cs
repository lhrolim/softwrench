using System;
using System.Globalization;
using System.Threading;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Mif {
    internal class MifMethodNameUtils {

        //        public static readonly string baseWsUrl = ApplicationConfiguration.WsUrl;
        public static readonly string baseWsPrefix = ApplicationConfiguration.WsPrefix;

        private static string GetProjectName() {
            return new MifUtils().GetType().Assembly.FullName.Split(',')[0];
        }

        //wsWorkorder, wsAsset
        public static string GetBaseWsTypeName(string entity) {
            TextInfo textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
            var titleCase = textInfo.ToTitleCase(entity);
            var projectName = GetProjectName();
            return projectName + ".ws" + titleCase;
        }

        public static string GetServiceName(string entity) {
            return GetBaseWsTypeName(entity) + "." + baseWsPrefix + entity.ToUpper();
        }

        public static String GetWsQueryTypeName(string entity) {
            return GetServiceName(entity) + "QueryType";
        }

        public static string GetWSEntityName(string entity) {
            return GetServiceName(entity) + "_" + entity.ToUpper() + "Type";
        }

        public static string GetMethodName(EntityMetadata entity, OperationType operation) {
            string prefix = "Sync";
            switch (operation) {
                case OperationType.Add:
                prefix = "Create";
                break;
                case OperationType.Change:
                prefix = "Sync";
                break;
                case OperationType.AddChange:
                prefix = "Sync";
                break;
                case OperationType.Delete:
                prefix = "Delete";
                break;
                case OperationType.Item:
                prefix = "Query";
                break;
            }

            var parameter = entity.ConnectorParameters.GetWSEntityKey(ConnectorParameters.UpdateInterfaceParam);
            if (parameter != null) {
                return prefix + parameter;
            }
            return prefix + baseWsPrefix + entity.Name.ToUpper();
        }
    }
}
