
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using cts.commons.web.Attributes;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
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
            var auditEntries = dao.FindByNativeQuery<AuditEntry>("select Id,Action,RefApplication,RefId,Data,CreatedBy,CreatedDate from Audit_Entry");
            return new GenericResponseResult<AuditEntryDto>(new AuditEntryDto { AuditEntries = auditEntries });
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