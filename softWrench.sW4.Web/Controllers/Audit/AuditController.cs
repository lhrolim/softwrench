
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using cts.commons.web.Attributes;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.SWDB;
using softwrench.sw4.Hapag.Data;
using softwrench.sw4.Hapag.Data.Init;
using softwrench.sw4.Hapag.Data.Sync;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Models.MyProfile;
using softWrench.sW4.Web.SPF;
using System.Text.RegularExpressions;
using cts.commons.portable.Util;
using softwrench.sW4.audit.classes.Model;

namespace softWrench.sW4.Web.Controllers.Audit {
    [Authorize]
    public class AuditController : ApiController {

        private SWDBHibernateDAO dao;

        public AuditController(SWDBHibernateDAO dao) {
            this.dao = dao;
        }

        [SPFRedirect(Title = "Audit Trail")]
        [HttpGet]
        public GenericResponseResult<AuditEntryDto> List(bool refreshData = true)
        {
            var auditEntries = dao.FindByQuery<AuditEntry>("from AuditEntry");
            var list = new List<AuditEntry>();
            //foreach (var auditEntry in auditEntries)
            //{

            //    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            //    byte[] bytes = encoding.GetBytes(auditEntry["Data"]);


            //    var newAudit = new AuditEntry(
            //        Int32.Parse(auditEntry["Id"]),
            //        auditEntry["Action"],
            //        auditEntry["RefApplication"],
            //        auditEntry["RefId"],
            //        auditEntry["Data"],
            //        auditEntry["CreatedBy"],
            //        DateTime.Parse(auditEntry["CreatedDate"]));

            //    list.Add(newAudit);
            //}
            return new GenericResponseResult<AuditEntryDto>(new AuditEntryDto { AuditEntries = list });
        }

        public class AuditEntryDto {
            private ICollection<AuditEntry> _auditEntries;

            public ICollection<AuditEntry> AuditEntries {
                get { return _auditEntries; }
                set { _auditEntries = value; }
            }

        }
    }
}