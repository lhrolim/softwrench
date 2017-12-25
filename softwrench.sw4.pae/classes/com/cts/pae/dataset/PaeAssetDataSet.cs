﻿using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using softwrench.sW4.audit.Interfaces;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;

namespace softwrench.sw4.pae.classes.com.cts.pae.dataset {

    public class PaeAssetDataSet : MaximoApplicationDataSet {

        private readonly IContextLookuper _contextLookuper;
        private readonly IAuditManager _auditManager;

        public PaeAssetDataSet(IContextLookuper contextLookuper, IAuditManager auditManager) {
            _contextLookuper = contextLookuper;
            _auditManager = auditManager;
        }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = await base.GetApplicationDetail(application, user, request);

            if (result == null || !_contextLookuper.LookupContext().ScanMode) {
                return result;
            }
            
            // Submit the requested record back to the database with updated audit date
            var json = JObject.Parse(JsonConvert.SerializeObject(result.ResultObject.Fields));
            await Execute(application, json, request.Id, "update", false, null,null);
            // Create an audit entry for the updated asset record
            _auditManager.CreateAuditEntry("scan", application.Name, request.Id, result.ResultObject.GetAttribute(result.Schema.UserIdFieldName) as string, JsonConvert.SerializeObject(result.ResultObject.Fields), DateTime.Now.FromServerToRightKind());

            return result;
        }

        public override string ApplicationName() {
            return "asset,transportation";
        }

        public override string ClientFilter() {
            return "pae";
        }
    }
}