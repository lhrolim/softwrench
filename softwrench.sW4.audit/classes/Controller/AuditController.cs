using System.Collections.Generic;
using System.Net.Mime;
using System.Web.Http;
using cts.commons.web.Attributes;
using Newtonsoft.Json.Linq;
using softwrench.sw4.audit.classes.Model;
using softwrench.sW4.audit.classes.Controller;
using softWrench.sW4.SPF;


namespace softwrench.sw4.audit.classes.Controller {

    [Authorize]
    [SWControllerConfiguration]
    class AuditController : ApiController
    {
        private readonly AuditManager _auditManager;

        public AuditController(AuditManager auditManager)
        {
            _auditManager = auditManager;
        }

        [HttpPost]
        public void InsertAudit(string application, string entityId, string action, JObject jsonData)
        {
            
        }
    }
}
