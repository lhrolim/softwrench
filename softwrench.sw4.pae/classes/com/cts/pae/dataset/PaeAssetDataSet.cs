using System;
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

    class PaeAssetDataSet : MaximoApplicationDataSet
    {
        private IContextLookuper _contextLookuper;
        private IAuditManager _auditManager;
        
        public PaeAssetDataSet(IContextLookuper contextLookuper, IAuditManager auditManager)
        {
            _contextLookuper = contextLookuper;
            _auditManager = auditManager;
        }

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request)
        {
            var result = base.GetApplicationDetail(application, user, request);


            if (result != null && _contextLookuper.LookupContext().ScanMode)
            {
                // Submit the requested record back to the database with updated audit date
                JObject json = JObject.Parse(JsonConvert.SerializeObject(result.ResultObject.Fields));
                Execute(application, json, request.Id, "update", false,null);
                // Create an audit entry for the updated asset record
                _auditManager.CreateAuditEntry("scan", application.Name, request.Id, result.ResultObject.GetAttribute(result.Schema.UserIdFieldName) as string, JsonConvert.SerializeObject(result.ResultObject.Fields), DateTime.Now.FromServerToRightKind());
            }

            return result;
        }

        public override string ApplicationName() {
            return "asset";
        }

        public override string ClientFilter() {
            return "pae";
        }
    }
}