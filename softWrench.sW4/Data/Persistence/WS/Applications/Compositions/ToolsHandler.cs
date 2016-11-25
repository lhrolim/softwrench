using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {
    class ToolsHandler {

        private const string ItemNumAttribute = "itemnum";
        private const string ItemSetIdAttribute = "itemsetid";
        private const string OrgIdAttribute = "orgid";
        private const string ToolRateAttribute = "toolrate";

        public static void HandleWoTools(CrudOperationData entity, object wo) {
            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();

            // Workorder id used for data association
            var recordKey = entity.UserId;

            // Filter work order tools for any new entries where tooltrans is null
            var Tools = (IEnumerable<CrudOperationData>)entity.GetRelationship("tooltrans");
            var newTools = Tools.Where(r => r.GetAttribute("tooltransid") == null);

            // Convert collection into array, if any are available
            var crudOperationData = newTools as CrudOperationData[] ?? newTools.ToArray();

            var parsedOperationData = new List<CrudOperationData>();
            var toolItemMetadata = MetadataProvider.Entity("toolitem");
            HandlerParseUtil.ParseUnmappedCompositionInline(crudOperationData, parsedOperationData, "#toollist_",
                delegate (CrudOperationData newcrudOperationData, JObject jsonObject) {

                    var toolItemAssoc = new CrudOperationData(null, new Dictionary<string, object>(), new Dictionary<string, object>(), toolItemMetadata, null);
                    toolItemAssoc.Fields[ItemNumAttribute] = jsonObject.Value<string>(ItemNumAttribute);
                    toolItemAssoc.Fields[ItemSetIdAttribute] = jsonObject.Value<string>(ItemSetIdAttribute);
                    newcrudOperationData.AssociationAttributes["tool_"] = toolItemAssoc;

                    newcrudOperationData.Fields[ItemNumAttribute] = jsonObject.Value<string>(ItemNumAttribute);
                    newcrudOperationData.Fields[ItemSetIdAttribute] = jsonObject.Value<string>(ItemSetIdAttribute);
                    newcrudOperationData.Fields[OrgIdAttribute] = jsonObject.Value<string>(OrgIdAttribute);

                    var toolRateString = jsonObject.Value<string>(ToolRateAttribute);

                    double toolRate = -1;
                    double.TryParse(toolRateString, out toolRate);
                    if (double.TryParse(toolRateString, out toolRate)) {
                        newcrudOperationData.Fields[ToolRateAttribute] = toolRate;
                    }
                });

            WsUtil.CloneArray(parsedOperationData, wo, "TOOLTRANS", delegate (object integrationObject, CrudOperationData crudData) {
                WsUtil.SetValueIfNull(integrationObject, "TOOLRATE", 0.00);
                WsUtil.SetValueIfNull(integrationObject, "TOOLQTY", 0);
                WsUtil.SetValueIfNull(integrationObject, "TOOLHRS", 0);
                WsUtil.SetValueIfNull(integrationObject, "LINECOST", 0.00);

                WsUtil.SetValue(integrationObject, "ORGID", entity.GetAttribute("orgid"));
                WsUtil.SetValue(integrationObject, "SITEID", entity.GetAttribute("siteid"));
                WsUtil.SetValue(integrationObject, "REFWO", recordKey);

                WsUtil.SetValue(integrationObject, "ENTERBY", user.Login);
                WsUtil.SetValue(integrationObject, "ENTERDATE", DateTime.Now.FromServerToRightKind(), true);

                WsUtil.SetValue(integrationObject, "TOOLTRANSID", -1);
                WsUtil.SetValue(integrationObject, "TRANSDATE", DateTime.Now.FromServerToRightKind(), true);

                ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Add.ToString());
            });
        }
    }
}
