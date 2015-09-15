using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.SWDB;

namespace softWrench.sW4.Web.Controllers {
    public class CommTemplateController : ApiController {

        private readonly SWDBHibernateDAO _swdbDao;

        public CommTemplateController(SWDBHibernateDAO dao) {
            _swdbDao = dao;
        }

        [HttpGet]
        public IApplicationResponse MergeTemplateDefinition(string templateId, string applicationName, string ApplicationId) {

        }
    }
}
