using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using softwrench.sw4.Shared2.Util;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {
    public class RelatedRecordHandler {


        public static void HandleRelatedRecords(MaximoOperationExecutionContext maximoOperation) {
            var parentData = (CrudOperationData)maximoOperation.OperationData;
            var relatedRecords = ((IEnumerable<CrudOperationData>)parentData.GetRelationship("relatedrecord_"))
                                    .Where(r => r.IsDirty)
                                    .ToList();

            if (!relatedRecords.Any()) return;

            var ticket = maximoOperation.IntegrationObject;
            var user = SecurityFacade.CurrentUser();

            // Check if SR must be created before creating relationship coming from Work Order
            var newSr = relatedRecords.FindAll(rr => rr.GetUnMappedAttribute("#createSr") != null && "true".Equals(rr.GetUnMappedAttribute("#createSr").ToString().ToLower()));
            if (newSr.Any()) {
                // create the new SR before trying to create a related record for it
                var newSrId = CreateSr(parentData, user);
                relatedRecords.FirstOrDefault(rr => "true".Equals(rr.GetUnMappedAttribute("#createSr").ToString().ToLower())).SetAttribute("RELATEDRECKEY", newSrId);
            }

            w.CloneArray(relatedRecords, ticket, "RELATEDRECORD", (relatedRecord, relatedRecordData) => {
                w.SetValue(relatedRecord, "RELATEDRECORDID", -1);
                w.SetValue(relatedRecord, "RELATETYPE", "FOLLOWUP");
                // current SR data
                w.SetValue(relatedRecord, "RECORDKEY", parentData.UserId);
                w.CopyFromRootEntity(ticket, relatedRecord, "SITEID", user.SiteId);
                w.CopyFromRootEntity(ticket, relatedRecord, "ORGID", user.OrgId);
                // related target data
                var relatedClass = relatedRecordData.GetStringAttribute("relatedrecclass").ToUpper();
                if (relatedClass.Equals("WORKORDER")) {
                    w.SetValueIfNull(relatedRecord, "RELATEDRECWOCLASS", relatedClass);
                    w.SetValueIfNull(relatedRecord, "RELATEDRECWONUM", relatedRecordData.GetAttribute("relatedreckey"));
                    //in case of workorder we cannot pass these values, but rather the WO specific ones
                    w.NullifyValue(relatedRecord, "RELATEDRECCLASS");
                    w.NullifyValue(relatedRecord, "RELATEDRECKEY");
                }
            });
        }

        // Create a new SR form the Work Order and return the ID for the relaterecords
        private static string CreateSr(CrudOperationData parentData, InMemoryUser user) {
            var schemaKey = SchemaUtil.GetSchemaKeyFromString("newdetail", ClientPlatform.Web);
            var application = MetadataProvider.Application("servicerequest").ApplyPolicies(schemaKey, user, ClientPlatform.Web);
            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var multiassetlocci = (List<CrudOperationData>)parentData.AssociationAttributes["multiassetlocci_"];
            

            var srData = new JObject();
            // If there are multiassetlocci, serialize and add to the srData
            if (multiassetlocci != null) {
                JArray multiassetloccis = new JArray();
                foreach (var value in multiassetlocci) {
                    value.Fields.Add(new KeyValuePair<string, object>("#isDirty", "true"));
                    value.Fields["recordclass"] = "SR";
                    value.Fields["recordkey"] = null;
                    var obj = new JObject();
                    foreach (var field in value.Fields) {
                        // Do not include the ID or null values
                        if (field.Value == null || field.Key == "multiid") {
                            continue;
                        }
                        obj[field.Key] = field.Value != null ? field.Value.ToString() : null;
                    }
                    multiassetloccis.Add(obj);
                }
                srData.Add("multiassetlocci_", multiassetloccis);
            }
            srData.Add("orgid", parentData.GetStringAttribute("orgid"));
            srData.Add("siteid", parentData.GetStringAttribute("siteid"));
            srData.Add("description", parentData.GetStringAttribute("description"));
            srData.Add("ld_.ldtext", parentData.GetStringAttribute("longdescription_.ldtext"));
            srData.Add("assetnum", parentData.GetStringAttribute("assetnum"));
            srData.Add("classstructureid", parentData.GetStringAttribute("classstructureid"));
            srData.Add("location", parentData.GetStringAttribute("location"));
            srData.Add("reportedby", parentData.GetStringAttribute("reportedby"));
            srData.Add("glaccount", parentData.GetStringAttribute("glaccount"));
            srData.Add("reportdate", DateTime.Now.FromServerToRightKind());
            srData.Add("affecteddate", DateTime.Now.FromServerToRightKind());
            srData.Add("affectedperson", parentData.GetStringAttribute("reportedby"));
            srData.Add("affectedphone", parentData.GetStringAttribute("phone"));
            srData.Add("affectedemail", parentData.GetStringAttribute("email"));
            srData.Add("reportedphone", parentData.GetStringAttribute("phone"));
            srData.Add("reportedemail", parentData.GetStringAttribute("email"));
            srData.Add("assetorgid", parentData.GetStringAttribute("orgid"));
            srData.Add("assetsiteid", parentData.GetStringAttribute("siteid"));
            //srData.Add("origrecordclass", "WORKORDER");
            //srData.Add("origrecordid", parentData.GetStringAttribute("wonum"));
            srData.Add("class", "SR");

            var operationWrapper = new OperationWrapper(application, entityMetadata, OperationConstants.CRUD_CREATE, srData, null);
            var engine = SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(typeof(MaximoConnectorEngine));
            var result = engine.Execute(operationWrapper);
            return result.UserId;
        }
    }
}
